using Application.Email;
using Application.Services.Interfaces;
using FeedBackApp.Core.Email;
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
    public class AuthFunctions
    {
        private readonly ILogger<AuthFunctions> _logger;
        private readonly IWhitelistRepository _whitelistRepository;
        private readonly IOtpService _otpService;
        private readonly IEmailContentService _emailContentService;
        private readonly IEmailSender _emailSender;

        public AuthFunctions(
            ILogger<AuthFunctions> logger,
            IWhitelistRepository whitelistRepository,
            IOtpService otpService,
            IEmailContentService emailContentService,
            IEmailSender emailSender)
        {
            _logger = logger;
            _whitelistRepository = whitelistRepository;
            _otpService = otpService;
            _emailContentService = emailContentService;
            _emailSender = emailSender;
        }

        /// <summary>
        /// Handles Google-based login (<c>POST</c>) and CORS preflight (<c>OPTIONS</c>).
        /// Validates the Google ID token and issues a secure JWT cookie.
        /// </summary>
        [Function("LoginWithGoogle")]
        [OpenApiOperation(operationId: "LoginWithGoogle", tags: new[] { "Auth" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(LoginRequest), Required = true, Description = "Google ID Token payload")]
        public async Task<HttpResponseData> LoginWithGoogle(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/google")] HttpRequestData req)
        {
            _logger.LogInformation("LoginWithGoogle function triggered.");

            // 1. Load Whitelist
            var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
            var students = whitelist?.StudentEmails ?? new List<string>();

            // 2. Handle CORS / Preflight
            var origin = GetOrigin(req);
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                return CreatePreflightResponse(req, origin);
            }

            // 3. Parse Body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<LoginRequest>(body);

            if (data is null || string.IsNullOrWhiteSpace(data.IdToken))
            {
                _logger.LogWarning("Login request missing or invalid IdToken");
                return CreateErrorResponse(req, System.Net.HttpStatusCode.BadRequest, "IdToken is required", origin);
            }

            // 4. Validate Google Token
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var googleClientId = Environment.GetEnvironmentVariable("GoogleClientId") 
                                     ?? Environment.GetEnvironmentVariable("Google:ClientId"); // Fallback for diff naming conventions

                payload = await GoogleJsonWebSignature.ValidateAsync(data.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                        Audience = [Environment.GetEnvironmentVariable("Google:ClientId")]
                });
                _logger.LogInformation("Google token validated. Email: {Email}", payload.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid Google token");
                return CreateErrorResponse(req, System.Net.HttpStatusCode.Unauthorized, "Invalid Google token", origin);
            }

            // 5. Authorize (Check Whitelist/Admin)
            bool isAdmin = IsAdmin(payload.Email);

            if (!IsAuthorizedEmail(payload.Email,students,isAdmin))
            {
                _logger.LogWarning("Unauthorized login attempt. Email: {Email}", payload.Email);
                return CreateErrorResponse(req, System.Net.HttpStatusCode.Forbidden, "User not found", origin);
            }

            // 6. Generate Token & Response
            return await CreateLoginResponse(req, payload.Email, payload.GivenName, payload.FamilyName, isAdmin, origin);
        }

        /// <summary>
        /// Sends a One-Time Password (OTP) to the specified email address if the user is authorized.
        /// </summary>
        [Function("SendOtp")]
        [OpenApiOperation(operationId: "SendOtp", tags: new[] { "Auth" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(SendOtpRequest), Required = true, Description = "Email address to send OTP to")]
        public async Task<HttpResponseData> SendOtp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/otp/send")] HttpRequestData req)
        {
            _logger.LogInformation("SendOtp function triggered.");

            // 1. Handle CORS
            var origin = GetOrigin(req);
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                return CreatePreflightResponse(req, origin);
            }

            // 2. Parse Body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<SendOtpRequest>(body);
            
            if (data is null || string.IsNullOrWhiteSpace(data.Email))
            {
                return CreateErrorResponse(req, System.Net.HttpStatusCode.BadRequest, "Email is required", origin);
            }

            var email = data.Email.Trim().ToLowerInvariant();

            if(!IsValidEmailFormat(email)){
                _logger.LogWarning("Invalid email format in OTP request: {Email}", email);
                return CreateErrorResponse(req, System.Net.HttpStatusCode.BadRequest, "Invalid email format", origin);
            }

            // 3. Check Authorization
            var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
            var students = whitelist?.StudentEmails ?? new List<string>();
            bool isAdmin = IsAdmin(email);

            // LOGIC FIX: Changed from (students.Contains && !isAdmin) to (!students.Contains && !isAdmin)
            if (!IsAuthorizedEmail(email,students,isAdmin))
            {
                _logger.LogWarning("Unauthorized OTP request. Email: {Email}", email);
                return CreateErrorResponse(req, System.Net.HttpStatusCode.Forbidden, "User not found", origin);
            }

            try
            {
                // 4. Generate & Send
                var otpCode = _otpService.GenerateOtp(email);
                _logger.LogInformation("Generated OTP for {Email}", email);

                var emailMessage = await _emailContentService.CreateOtpEmailAsync(email, otpCode);
                var emailSent = await _emailSender.SendEmailAsync(emailMessage);

                if (!emailSent)
                {
                    _logger.LogError("Failed to send OTP email to {Email}", email);
                    return CreateErrorResponse(req, System.Net.HttpStatusCode.InternalServerError, "Failed to send email", origin);
                }

                _logger.LogInformation("OTP email sent successfully to {Email}", email);
                
                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                AddCorsHeaders(response, origin);
                await response.WriteAsJsonAsync(new { message = "OTP sent successfully" });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP to {Email}", email);
                return CreateErrorResponse(req, System.Net.HttpStatusCode.InternalServerError, "An error occurred while sending OTP", origin);
            }
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
            var students = whitelist?.StudentEmails ?? new List<string>();

            bool isAdmin = IsAdmin(email);

            if (!IsAuthorizedEmail(email,students,isAdmin))
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
            var students = whitelist?.StudentEmails ?? new List<string>();

            bool isAdmin = IsAdmin(email);

            if (!IsAuthorizedEmail(email,students,isAdmin))
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

        [Function("LoginWithLinkedIn")]
        public async Task<HttpResponseData> LoginWithLinkedIn(
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/linkedin")]
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
            var data = JsonConvert.DeserializeObject<LinkedInLoginRequest>(body);

            if (string.IsNullOrWhiteSpace(data?.AuthCode))
            {
                var bad = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("AuthCode required");
                return bad;
            }

            using var http = new HttpClient();
            var clientId = Environment.GetEnvironmentVariable("LinkedIn:ClientId");
            var clientSecret = Environment.GetEnvironmentVariable("LinkedIn:ClientSecret");
            var redirectUri = Environment.GetEnvironmentVariable("LinkedIn:RedirectUri");

            var tokenRequestParams = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = data.AuthCode,
                ["redirect_uri"] = redirectUri,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret
            };

            var tokenResponse = await http.PostAsync("https://www.linkedin.com/oauth/v2/accessToken",
                                new FormUrlEncodedContent(tokenRequestParams));

            if(!tokenResponse.IsSuccessStatusCode)
            {
                var unauth = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauth.WriteStringAsync("Invalid LinkedIn authorization code");
                return unauth;
            }

            var tokenBody = await tokenResponse.Content.ReadAsStringAsync();
            dynamic? tokenData = JsonConvert.DeserializeObject(tokenBody);
            string? accessToken = tokenData?.access_token;

            if(string.IsNullOrWhiteSpace(accessToken))
            {
                var unauth = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                await unauth.WriteStringAsync("Invalid LinkedIn authorization code");
                return unauth;
            }

            // userInfo lekerese
            var userInfoRequest = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/userinfo");
            userInfoRequest.Headers.Add("Authorization", $"Bearer {accessToken}");
            var userInfoHttpResponse = await http.SendAsync(userInfoRequest);
            var userInfoBody = await userInfoHttpResponse.Content.ReadAsStringAsync();
            dynamic? userInfo = JsonConvert.DeserializeObject(userInfoBody);

            string? email = userInfo?.email;
            if (string.IsNullOrWhiteSpace(email))
            {
                // Felhasználó nem engedélyezte az email megosztást
                var forbidden = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                await forbidden.WriteStringAsync("Email not available. Please allow email access in LinkedIn login.");
                return forbidden;
            }

            //  Authorization (student/admin)
            var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
            var students = whitelist?.StudentEmails ?? new List<string>();

            bool isAdmin = IsAdmin(email);

            if (!IsAuthorizedEmail(email,students,isAdmin))
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
                firstName = userInfo?.given_name,
                lastName = userInfo?.family_name,
                role = isAdmin ? "Admin" : "Student",
                provider = "LinkedIn"
            });

            return response;
        }

        /// <summary>
        /// Verifies the provided OTP code and issues a JWT if valid.
        /// </summary>
        [Function("VerifyOtp")]
        [OpenApiOperation(operationId: "VerifyOtp", tags: new[] { "Auth" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(VerifyOtpRequest), Required = true, Description = "Email and OTP code for verification")]
        public async Task<HttpResponseData> VerifyOtp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/otp/verify")] HttpRequestData req)
        {
            _logger.LogInformation("VerifyOtp function triggered.");

            // 1. Handle CORS
            var origin = GetOrigin(req);
            if (req.Method.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase))
            {
                return CreatePreflightResponse(req, origin);
            }

            // 2. Parse Body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<VerifyOtpRequest>(body);
            
            if (data is null || string.IsNullOrWhiteSpace(data.Email) || string.IsNullOrWhiteSpace(data.Code))
            {
                return CreateErrorResponse(req, System.Net.HttpStatusCode.BadRequest, "Email and code are required", origin);
            }

            var email = data.Email.Trim().ToLowerInvariant();
            var code = data.Code.Trim();

            // 3. Validate OTP Logic
            var isValid = _otpService.ValidateOtp(email, code);
            if (!isValid)
            {
                _logger.LogWarning("OTP validation failed for {Email}", email);
                return CreateErrorResponse(req, System.Net.HttpStatusCode.Unauthorized, "Invalid or expired OTP code", origin);
            }

            // 4. Re-check Authorization (Safety check)
            var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
            var students = whitelist?.StudentEmails ?? new List<string>();
            bool isAdmin = IsAdmin(email);

            if (!IsAuthorizedEmail(email,students,isAdmin))
            {
                _logger.LogWarning("User not in whitelist after OTP validation. Email: {Email}", email);
                return CreateErrorResponse(req, System.Net.HttpStatusCode.Forbidden, "User not found", origin);
            }

            // 5. Cleanup & Token Generation
            _otpService.RemoveOtp(email);
            
            // Note: OTP login doesn't provide names, so we leave them null or use placeholders
            return await CreateLoginResponse(req, email, null, null, isAdmin, origin);
        }

        #region Helper Methods

        private bool IsAdmin(string email)
        {
            var adminEmailsEnv = Environment.GetEnvironmentVariable("AdminEmails") ?? "";
            var adminEmails = adminEmailsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase);
        }

        private bool IsAuthorizedEmail(string email, IReadOnlyCollection<string> students, bool isAdmin)
        {
            if(isAdmin) return true;
            if(!IsWhitelistRequired()) return true;
            return students.Contains(email, StringComparer.OrdinalIgnoreCase);
        }

        private bool IsWhitelistRequired(){
            var requirement = Environment.GetEnvironmentVariable("RequireStudentWhitelist");
            return !string.Equals(requirement, "false", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsValidEmailFormat(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch(FormatException)
            {
                return false;
            }
        }

        private string GenerateJwtToken(string email, bool isAdmin)
        {
            string secretKey = Environment.GetEnvironmentVariable("JwtSecretKey") 
                               ?? Environment.GetEnvironmentVariable("Jwt:SecretKey")
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

        private string? GetOrigin(HttpRequestData req)
        {
            return req.Headers.TryGetValues("Origin", out var origins) ? origins.FirstOrDefault() : null;
        }

        private HttpResponseData CreatePreflightResponse(HttpRequestData req, string? origin)
        {
            _logger.LogInformation("Handling CORS preflight request");
            var resp = req.CreateResponse(System.Net.HttpStatusCode.NoContent);
            AddCorsHeaders(resp, origin);
            return resp;
        }

        private HttpResponseData CreateErrorResponse(HttpRequestData req, System.Net.HttpStatusCode code, string message, string? origin)
        {
            var resp = req.CreateResponse(code);
            AddCorsHeaders(resp, origin);
            // Fire and forget waiting for write, or await it. Since this is a helper returning HttpResponseData, 
            // we can't await WriteStringAsync easily without refactoring. 
            // Instead, we explicitly write to the body stream or use a small workaround.
            // For simplicity in this helper, we'll write sync or use a wrapping task in the caller.
            // BUT, strictly speaking, WriteAsJsonAsync is easiest.
            resp.WriteAsJsonAsync(new { message }).GetAwaiter().GetResult(); 
            return resp;
        }

        private async Task<HttpResponseData> CreateLoginResponse(HttpRequestData req, string email, string? firstName, string? lastName, bool isAdmin, string? origin)
        {
            _logger.LogInformation("User authenticated. Email: {Email}, Role: {Role}", email, isAdmin ? "Admin" : "Student");

            var token = GenerateJwtToken(email, isAdmin);
            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            
            AddCorsHeaders(response, origin);

            response.Headers.Add("Set-Cookie", $"token={token}; HttpOnly; SameSite=None; Secure; Path=/; Max-Age=86400");

            await response.WriteAsJsonAsync(new
            {
                email = email,
                firstName = firstName,
                lastName = lastName,
                role = isAdmin ? "Admin" : "Student"
            });
            
            return response;
        }

        private void AddCorsHeaders(HttpResponseData resp, string? origin)
        {
            if (!string.IsNullOrEmpty(origin))
            {
                resp.Headers.Add("Access-Control-Allow-Origin", origin);
                resp.Headers.Add("Access-Control-Allow-Methods", "POST, OPTIONS");
                resp.Headers.Add("Access-Control-Allow-Headers", "Content-Type");
                resp.Headers.Add("Access-Control-Allow-Credentials", "true");
            }
        }

        #endregion

        #region DTOs
        public class LoginRequest
        {
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
        public class LinkedInLoginRequest
        {
            public required string AuthCode { get; set; }
        }

        public class SendOtpRequest
        {
            public required string Email { get; set; }
        }

        public class VerifyOtpRequest
        {
            public required string Email { get; set; }
            public required string Code { get; set; }
        }
        #endregion
    }
}