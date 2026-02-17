using Application.Email;
using Application.Services.Interfaces;
using FeedBackApp.Core.Email;
using FeedBackApp.Core.Repositories;
using Google.Apis.Auth;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

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
        /// Supports self opt-in workflow via AllowSelfOptIn flag.
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
                                     ?? Environment.GetEnvironmentVariable("Google:ClientId");

                payload = await GoogleJsonWebSignature.ValidateAsync(data.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                });
                _logger.LogInformation("Google token validated. Email: {Email}", payload.Email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Invalid Google token");
                return CreateErrorResponse(req, System.Net.HttpStatusCode.Unauthorized, "Invalid Google token", origin);
            }

            // 5. Authorize (Check Whitelist/Admin) - WITH SELF OPT-IN SUPPORT
            bool isAdmin = IsAdmin(payload.Email);

            // If allowSelfOptIn is true, skip whitelist check
            // This allows users with opt-in tokens to log in even if not whitelisted
            if (!data.AllowSelfOptIn)
            {
                if (!students.Contains(payload.Email, StringComparer.OrdinalIgnoreCase) && !isAdmin)
                {
                    _logger.LogWarning("Unauthorized login attempt. Email: {Email}", payload.Email);
                    return CreateErrorResponse(req, System.Net.HttpStatusCode.Forbidden, "User not found", origin);
                }
            }
            else
            {
                _logger.LogInformation("Self opt-in login allowed for Email: {Email}", payload.Email);
            }

            // 6. Generate Token & Response
            return await CreateLoginResponse(req, payload.Email, payload.GivenName, payload.FamilyName, isAdmin, origin);
        }

        /// <summary>
        /// Sends a One-Time Password (OTP) to the specified email address.
        /// Supports self opt-in workflow via AllowSelfOptIn flag.
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

            // 3. Check Authorization with self opt in 
            var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
            var students = whitelist?.StudentEmails ?? new List<string>();
            bool isAdmin = IsAdmin(email);

            // If allowSelfOptIn is true, skip whitelist check
            if (!data.AllowSelfOptIn)
            {
                if (!students.Contains(email, StringComparer.OrdinalIgnoreCase) && !isAdmin)
                {
                    _logger.LogWarning("Unauthorized OTP request. Email: {Email}", email);
                    return CreateErrorResponse(req, System.Net.HttpStatusCode.Forbidden, "User not found", origin);
                }
            }
            else
            {
                _logger.LogInformation("Self opt-in OTP send allowed for Email: {Email}", email);
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

        /// <summary>
        /// Verifies the provided OTP code and issues a JWT if valid.
        /// Supports self opt-in workflow - whitelist check happens in SendOtp.
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

            // 4. NO whitelist check here - authorization was done in SendOtp
            // If user received an OTP, they are authorized to verify it
            bool isAdmin = IsAdmin(email);

            // 5. Cleanup & Token Generation
            _otpService.RemoveOtp(email);

            return await CreateLoginResponse(req, email, null, null, isAdmin, origin);
        }

        #region Helper Methods

        private bool IsAdmin(string email)
        {
            var adminEmailsEnv = Environment.GetEnvironmentVariable("AdminEmails") ?? "";
            var adminEmails = adminEmailsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase);
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
                new Claim("email", email),
                new Claim(ClaimTypes.Email, email),
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
            public bool AllowSelfOptIn { get; set; } = false;
        }

        public class SendOtpRequest
        {
            public required string Email { get; set; }
            public bool AllowSelfOptIn { get; set; } = false;
        }

        public class VerifyOtpRequest
        {
            public required string Email { get; set; }
            public required string Code { get; set; }
        }
        #endregion
    }
}