using FeedBackApp.Core.Repositories;
using Google.Apis.Auth;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HttpRequestData = Microsoft.Azure.Functions.Worker.Http.HttpRequestData;


namespace AzureFunctionsAPI.AzureEndPointReaction.Functions
{
    /// <summary>
    /// Authentication endpoints for the School Feedback application (Azure Functions – .NET isolated worker).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Overview</b><br/>
    /// This function class implements a Google-based login flow and issues a server-generated JWT which is returned
    /// to the browser as a secure, HTTP-only cookie. The endpoint also performs application-level authorization
    /// by validating the caller against a student whitelist and/or an administrator list supplied via environment variables.
    /// </para>
    ///
    /// <para>
    /// <b>End-to-end flow</b>
    /// <list type="number">
    ///   <item><description><b>CORS &amp; preflight</b>: For <c>OPTIONS</c> requests the function returns a <c>204 No Content</c> with appropriate <c>Access-Control-*</c> headers.</description></item>
    ///   <item><description><b>Request parsing</b>: On <c>POST</c> requests the function reads a JSON body into <see cref="LoginRequest"/> and ensures a non-empty Google <c>IdToken</c>.</description></item>
    ///   <item><description><b>Google token validation</b>: The function verifies the <c>IdToken</c> via <see cref="GoogleJsonWebSignature.ValidateAsync(string, GoogleJsonWebSignature.ValidationSettings)"/>,
    ///   constrained by the configured <c>GoogleClientId</c> (<c>Audience</c>).</description></item>
    ///   <item><description><b>Authorization</b>: The caller's email is checked against a student whitelist (repository-backed) and a comma-separated admin list from <c>AdminEmails</c> environment variable.</description></item>
    ///   <item><description><b>JWT issuance</b>: A short, role-bearing JWT is created with <c>HS256</c> using <c>JwtSecretKey</c>. Claims include <c>NameIdentifier</c> (email) and <c>Role</c> (<c>Admin</c>|<c>Student</c>).</description></item>
    ///   <item><description><b>Cookie + JSON</b>: The JWT is set as an HTTP-only, <c>SameSite=None</c>, <c>Secure</c> cookie (<c>token</c>) with 1-day lifetime. The body returns a minimal user profile (email, first/last name, role).</description></item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// <b>Security notes</b>
    /// <list type="bullet">
    ///   <item><description>JWT signing uses a symmetric key (<c>HS256</c>). Keep <c>JwtSecretKey</c> secret and sufficiently long (min. 32 random bytes recommended).</description></item>
    ///   <item><description>Cookie is <c>HttpOnly</c> + <c>Secure</c> + <c>SameSite=None</c> to support cross-site scenarios with credentials while mitigating XSS and ensuring TLS-only transport.</description></item>
    ///   <item><description>Origin is echoed to <c>Access-Control-Allow-Origin</c> from request header; in production, validate the origin against an allowlist.</description></item>
    ///   <item><description>Google ID token is validated for audience binding (<c>GoogleClientId</c>).</description></item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// <b>Environment variables</b>
    /// <list type="bullet">
    ///   <item><description><c>GoogleClientId</c>: OAuth 2.0 Client ID used as audience constraint when validating Google ID tokens.</description></item>
    ///   <item><description><c>AdminEmails</c>: Comma-separated list of admin email addresses (case-insensitive match).</description></item>
    ///   <item><description><c>JwtSecretKey</c>: Symmetric key for signing JWTs with HS256.</description></item>
    /// </list>
    /// </para>
    ///
    /// <para>
    /// <b>Logging</b><br/>
    /// The function emits structured logs for: invocation start, CORS handling, token validation, authorization decisions, and JWT issuance.
    /// </para>
    ///
    /// <para>
    /// <b>Response summary</b>
    /// <list type="bullet">
    ///   <item><description><c>204 No Content</c> – Preflight handled</description></item>
    ///   <item><description><c>400 Bad Request</c> – Missing or empty <c>IdToken</c></description></item>
    ///   <item><description><c>401 Unauthorized</c> – Invalid Google token</description></item>
    ///   <item><description><c>403 Forbidden</c> – Email not authorized (neither student nor admin)</description></item>
    ///   <item><description><c>200 OK</c> – JWT issued (cookie) and user info returned in body</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    /// <param name="logger">Application logger used for structured diagnostics and traceability.</param>
    /// <param name="whitelistRepository">Repository providing access to the student email whitelist.</param>
    public class AuthFunctions(ILogger<AuthFunctions> logger, IWhitelistRepository whitelistRepository)
    {
        private readonly ILogger<AuthFunctions> _logger = logger;
        private readonly IWhitelistRepository _whitelistRepository = whitelistRepository;

        /// <summary>
        /// Handles Google-based login (<c>POST</c>) and CORS preflight (<c>OPTIONS</c>) for the <c>/api/auth/google</c> endpoint.
        /// </summary>
        /// <remarks>
        /// <para>
        /// POST: Expects a JSON body containing <see cref="LoginRequest.IdToken"/> (Google ID token). Validates the token's audience
        /// against the <c>GoogleClientId</c> environment variable. On success, authorizes the email against the whitelist/admin list,
        /// issues a role-bearing JWT, sets it as a secure HTTP-only cookie (<c>token</c>), and returns a small profile JSON.
        /// </para>
        /// <para>
        /// OPTIONS: Responds to preflight with <c>204 No Content</c> and CORS headers (<c>Access-Control-Allow-Origin</c>, <c>Allow-Methods</c>, <c>Allow-Headers</c>, <c>Allow-Credentials</c>).
        /// </para>
        /// </remarks>
        /// <param name="req">HTTP request containing the JSON payload and the <c>Origin</c> header used for CORS.</param>
        /// <returns>
        /// <see cref="HttpResponseData"/> with one of the following status codes:
        /// <list type="bullet">
        ///   <item><description><c>204 No Content</c> – Preflight handled</description></item>
        ///   <item><description><c>400 Bad Request</c> – Missing or empty <c>IdToken</c></description></item>
        ///   <item><description><c>401 Unauthorized</c> – Invalid Google token</description></item>
        ///   <item><description><c>403 Forbidden</c> – Email not authorized (neither student nor admin)</description></item>
        ///   <item><description><c>200 OK</c> – JWT issued (cookie) and user info returned in body</description></item>
        /// </list>
        /// </returns>
        [Function("LoginWithGoogle")]
        [OpenApiOperation(operationId: "LoginWithGoogle", tags: ["Auth"])]
        public async Task<HttpResponseData> LoginWithGoogle(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/google")] HttpRequestData req)
        {
            _logger.LogInformation("LoginWithGoogle function triggered.");

            var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
            var students = whitelist.StudentEmails;

            // Origin for CORS
            var origin = req.Headers.TryGetValues("Origin", out var origins) ? origins.FirstOrDefault() : null;
            _logger.LogDebug("Request origin: {Origin}", origin ?? "None");

            // CORS preflight
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Handling CORS preflight request");
                var preflight = req.CreateResponse(System.Net.HttpStatusCode.NoContent);
                if (!string.IsNullOrEmpty(origin))
                {
                    preflight.Headers.Add("Access-Control-Allow-Origin", origin);
                    preflight.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
                    preflight.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                    preflight.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                return preflight;
            }

            // Parse body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<LoginRequest>(body);
            if (data is null || string.IsNullOrWhiteSpace(data.IdToken))
            {
                _logger.LogWarning("Login request missing or invalid IdToken");
                var badReq = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                if (!string.IsNullOrEmpty(origin))
                {
                    badReq.Headers.Add("Access-Control-Allow-Origin", origin);
                    badReq.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                await badReq.WriteStringAsync("IdToken is required");
                return badReq;
            }

            // Validate Google ID token
            GoogleJsonWebSignature.Payload payload;
            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(
                    data.IdToken,
                    new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = [Environment.GetEnvironmentVariable("Google:ClientId")]
                    });

                _logger.LogInformation("Google token validated. Email: {Email}", payload.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid Google token");
                var badResp = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                if (!string.IsNullOrEmpty(origin))
                {
                    badResp.Headers.Add("Access-Control-Allow-Origin", origin);
                    badResp.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                await badResp.WriteStringAsync("Invalid Google token");
                return badResp;
            }

            // Authorization: student or admin
            var adminEmailsEnv = Environment.GetEnvironmentVariable("AdminEmails") ?? "";
            var adminEmails = adminEmailsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            bool isAdmin = adminEmails.Contains(payload.Email, StringComparer.OrdinalIgnoreCase);

            if (!students.Contains(payload.Email, StringComparer.OrdinalIgnoreCase) && !isAdmin)
            {
                _logger.LogWarning("Unauthorized login attempt. Email: {Email}", payload.Email);
                var notFoundResp = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                if (!string.IsNullOrEmpty(origin))
                {
                    notFoundResp.Headers.Add("Access-Control-Allow-Origin", origin);
                    notFoundResp.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                await notFoundResp.WriteStringAsync("User not found");
                return notFoundResp;
            }

            _logger.LogInformation("User authenticated. Email: {Email}, Role: {Role}", payload.Email, isAdmin ? "Admin" : "Student");

            var token = GenerateJwtToken(payload.Email, isAdmin);
            _logger.LogDebug("JWT generated for {Email}", payload.Email);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

            // CORS for credentialed response
            if (!string.IsNullOrEmpty(origin))
            {
                response.Headers.Add("Access-Control-Allow-Origin", origin);
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }

            // Secure cookie with JWT
            response.Headers.Add(
                "Set-Cookie",
                $"token={token}; HttpOnly; SameSite=None; Secure; Path=/; Max-Age=86400");

            await response.WriteAsJsonAsync(new
            {
                email = payload.Email,
                firstName = payload.GivenName,
                lastName = payload.FamilyName,
                role = isAdmin ? "Admin" : "Student"
            });

            _logger.LogInformation("LoginWithGoogle function completed successfully for {Email}", payload.Email);
            return response;
        }

        [Function("LoginWithFacebook")]
        public async Task<HttpResponseData> LoginWithFacebook(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/facebook")]
    HttpRequestData req)
        {
            var origin = req.Headers.TryGetValues("Origin", out var origins) ? origins.FirstOrDefault() : null;

            // CORS preflight
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var preflight = req.CreateResponse(System.Net.HttpStatusCode.NoContent);
                if (!string.IsNullOrEmpty(origin))
                {
                    preflight.Headers.Add("Access-Control-Allow-Origin", origin);
                    preflight.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
                    preflight.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                    preflight.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                return preflight;
            }

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<FacebookLoginRequest>(body);

            if (string.IsNullOrWhiteSpace(data?.AccessToken))
            {
                var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("AccessToken required");
                return bad;
            }

            using var http = new HttpClient();
            var appId = Environment.GetEnvironmentVariable("Facebook:AppId");
            var appSecret = Environment.GetEnvironmentVariable("Facebook:AppSecret");

            //  Validate access token
            var debugUrl = $"https://graph.facebook.com/debug_token?input_token={data.AccessToken}&access_token={appId}|{appSecret}";
            var debugResponse = await http.GetStringAsync(debugUrl);
            dynamic? debug = JsonConvert.DeserializeObject(debugResponse);

            if (debug?.data?.is_valid != true)
            {
                var unauth = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauth.WriteStringAsync("Invalid Facebook token");
                return unauth;
            }

            //  Fetch user profile (including email)
            var userInfoUrl = $"https://graph.facebook.com/me?fields=id,first_name,last_name,email&access_token={data.AccessToken}";
            var userInfoResponse = await http.GetStringAsync(userInfoUrl);
            dynamic? userInfo = JsonConvert.DeserializeObject(userInfoResponse);

            string? email = userInfo?.email;
            if (string.IsNullOrWhiteSpace(email))
            {
                // Felhasználó nem engedélyezte az email megosztást
                var forbidden = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Email not available. Please allow email access in Facebook login.");
                return forbidden;
            }

            //  Authorization (student/admin)
            var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
            var students = whitelist.StudentEmails;

            var adminEmailsEnv = Environment.GetEnvironmentVariable("AdminEmails") ?? "";
            var adminEmails = adminEmailsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            bool isAdmin = adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase);
            bool isStudent = students.Contains(email, StringComparer.OrdinalIgnoreCase);

            if (!isAdmin && !isStudent)
            {
                var notFoundResp = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                if (!string.IsNullOrEmpty(origin))
                {
                    notFoundResp.Headers.Add("Access-Control-Allow-Origin", origin);
                    notFoundResp.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                await notFoundResp.WriteStringAsync("User not authorized");
                return notFoundResp;
            }

            //  JWT issuance
            var token = GenerateJwtToken(email, isAdmin);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

            if (!string.IsNullOrEmpty(origin))
            {
                response.Headers.Add("Access-Control-Allow-Origin", origin);
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }

            response.Headers.Add(
                "Set-Cookie",
                $"token={token}; HttpOnly; SameSite=None; Secure; Path=/; Max-Age=86400"
            );

            await response.WriteAsJsonAsync(new
            {
                email,
                firstName = userInfo?.first_name,
                lastName = userInfo?.last_name,
                role = isAdmin ? "Admin" : "Student",
                provider = "Facebook"
            });

            return response;
        }

        [Function("LoginWithMicrosoft")]
        public async Task<HttpResponseData> LoginWithMicrosoft(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/microsoft")]
    HttpRequestData req)
        {
            var origin = req.Headers.TryGetValues("Origin", out var origins)
                ? origins.FirstOrDefault()
                : null;


            // CORS preflight

            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                var preflight = req.CreateResponse(System.Net.HttpStatusCode.NoContent);
                if (!string.IsNullOrEmpty(origin))
                {
                    preflight.Headers.Add("Access-Control-Allow-Origin", origin);
                    preflight.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
                    preflight.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                    preflight.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                return preflight;
            }


            // Parse request body

            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<MicrosoftLoginRequest>(body);

            if (string.IsNullOrWhiteSpace(data?.IdToken))
            {
                var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("IdToken is required");
                return bad;
            }


            // Microsoft ID token validation 

            ClaimsPrincipal principal;
            try
            {
                var tenantId = Environment.GetEnvironmentVariable("Microsoft:TenantId") ?? "common";
                var clientId = Environment.GetEnvironmentVariable("Microsoft:ClientId");

                var authority = $"https://login.microsoftonline.com/{tenantId}/v2.0";
                var metadataAddress = $"{authority}/.well-known/openid-configuration";

                var configManager =
                    new ConfigurationManager<OpenIdConnectConfiguration>(
                        metadataAddress,
                        new OpenIdConnectConfigurationRetriever()
                    );

                var openIdConfig = await configManager.GetConfigurationAsync();

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = $"https://login.microsoftonline.com/{tenantId}/v2.0",

                    ValidateAudience = true,
                    ValidAudience = clientId,

                    ValidateLifetime = true,
                    IssuerSigningKeys = openIdConfig.SigningKeys

                };

                var handler = new JwtSecurityTokenHandler();
                principal = handler.ValidateToken(data.IdToken, validationParameters, out _);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid Microsoft token");

                var unauth = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauth.WriteStringAsync("Invalid Microsoft token");
                return unauth;
            }


            // Extract email

            var email =
                principal.FindFirst(ClaimTypes.Email)?.Value ??
                principal.FindFirst("preferred_username")?.Value;

            if (string.IsNullOrWhiteSpace(email))
            {
                var forbidden = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Email not available from Microsoft account");
                return forbidden;
            }


            // Authorization (UGYANAZ, mint Google/Facebook)

            var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
            var students = whitelist.StudentEmails;

            var adminEmailsEnv = Environment.GetEnvironmentVariable("AdminEmails") ?? "";
            var adminEmails = adminEmailsEnv
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            bool isAdmin = adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase);
            bool isStudent = students.Contains(email, StringComparer.OrdinalIgnoreCase);

            if (!isAdmin && !isStudent)
            {
                var forbidden = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("User not authorized");
                return forbidden;
            }

            // JWT issuance (UGYANAZ)

            var token = GenerateJwtToken(email, isAdmin);

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);

            if (!string.IsNullOrEmpty(origin))
            {
                response.Headers.Add("Access-Control-Allow-Origin", origin);
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }

            response.Headers.Add(
                "Set-Cookie",
                $"token={token}; HttpOnly; SameSite=None; Secure; Path=/; Max-Age=86400"
            );

            await response.WriteAsJsonAsync(new
            {
                email,
                role = isAdmin ? "Admin" : "Student",
                provider = "Microsoft"
            });

            return response;
        }


        /// <summary>
        /// Generates an HS256-signed JWT for the specified user, embedding identity and role claims.
        /// </summary>
        /// <remarks>
        /// The token contains the <c>ClaimTypes.NameIdentifier</c> (user email) and <c>ClaimTypes.Role</c> (<c>Admin</c> or <c>Student</c>) claims.
        /// Issuer and audience are both set to <c>SchoolFeedbackWebAPI</c>. The token lifetime is 7 days.
        /// The signing key is loaded from the <c>JwtSecretKey</c> environment variable.
        /// </remarks>
        /// <param name="email">User email to embed as the name identifier claim.</param>
        /// <param name="isAdmin">Determines the role claim (<c>Admin</c> if <c>true</c>, otherwise <c>Student</c>).</param>
        /// <returns>A compact JWS (JWT) string signed with HS256.</returns>
        /// <exception cref="InvalidOperationException">Thrown when <c>JwtSecretKey</c> is not configured.</exception>
        private string GenerateJwtToken(string email, bool isAdmin)
        {
            string secretKey = Environment.GetEnvironmentVariable("Jwt:SecretKey") ?? throw (new InvalidOperationException("JwtSecretKey environment variable not set."))
                ?? throw new InvalidOperationException("JwtSecretKey environment variable not set.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, email),
                new Claim(ClaimTypes.Role, isAdmin ? "Admin" : "Student")
            };

            var token = new JwtSecurityToken(
                issuer: "SchoolFeedbackWebAPI",
                audience: "SchoolFeedbackWebAPI",
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Incoming JSON payload carrying the Google ID token to validate.
        /// </summary>
        /// <remarks>
        /// The <see cref="IdToken"/> must be a Google-issued ID token for the configured <c>GoogleClientId</c> audience.
        /// </remarks>
        public class LoginRequest
        {
            /// <summary>
            /// Google ID token (JWT) obtained on the client via Google Sign-In.
            /// </summary>
            public required string IdToken { get; set; }
        }
        public class FacebookLoginRequest
        {
            public required string AccessToken { get; set; }
        }
        public class MicrosoftLoginRequest
        {
            public required string IdToken { get; set; }
        }

    }
}
