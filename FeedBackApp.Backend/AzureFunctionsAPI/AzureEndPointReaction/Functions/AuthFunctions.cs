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

        [Function("LoginWithGoogle")]
        [OpenApiOperation(operationId: "LoginWithGoogle", tags: new[] { "Auth" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(LoginRequest), Required = true, Description = "Google ID Token payload")]
        public async Task<HttpResponseData> LoginWithGoogle(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/google")] HttpRequestData req)
        {
            _logger.LogInformation("LoginWithGoogle function triggered.");

            var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
            var students = whitelist?.StudentEmails ?? new List<string>();

            // Get origin
            var origin = req.Headers.TryGetValues("Origin", out var origins) ? origins.FirstOrDefault() : null;
            _logger.LogDebug("Request origin: {Origin}", origin ?? "None");

            // Handle preflight request
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

            // Read POST body
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
            GoogleJsonWebSignature.Payload payload;

            try
            {
                payload = await GoogleJsonWebSignature.ValidateAsync(data.IdToken, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { Environment.GetEnvironmentVariable("GoogleClientId") }
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

            // Check if student or admin
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

            // Set CORS headers for credentials
            if (!string.IsNullOrEmpty(origin))
            {
                response.Headers.Add("Access-Control-Allow-Origin", origin);
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }

            // Set HttpOnly cookie
            response.Headers.Add("Set-Cookie",
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

        private string GenerateJwtToken(string email, bool isAdmin)
        {
            string secretKey = Environment.GetEnvironmentVariable("JwtSecretKey") ?? throw (new InvalidOperationException("JwtSecretKey environment variable not set."));
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

        [Function("SendOtp")]
        [OpenApiOperation(operationId: "SendOtp", tags: new[] { "Auth" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(SendOtpRequest), Required = true, Description = "Email address to send OTP to")]
        public async Task<HttpResponseData> SendOtp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/otp/send")] HttpRequestData req)
        {
            _logger.LogInformation("SendOtp function triggered.");

            var origin = req.Headers.TryGetValues("Origin", out var origins) ? origins.FirstOrDefault() : null;
            _logger.LogDebug("Request origin: {Origin}", origin ?? "None");

            // Handle preflight request
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

            // Read POST body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<SendOtpRequest>(body);
            
            if (data is null || string.IsNullOrWhiteSpace(data.Email))
            {
                _logger.LogWarning("SendOtp request missing or invalid email");
                var badReq = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                if (!string.IsNullOrEmpty(origin))
                {
                    badReq.Headers.Add("Access-Control-Allow-Origin", origin);
                    badReq.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                await badReq.WriteAsJsonAsync(new { message = "Email is required" });
                return badReq;
            }

            var email = data.Email.Trim().ToLowerInvariant();

            // Check if email is in whitelist or is admin
            var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
            var students = whitelist?.StudentEmails ?? new List<string>();
            var adminEmailsEnv = Environment.GetEnvironmentVariable("AdminEmails") ?? "";
            var adminEmails = adminEmailsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            bool isAdmin = adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase);

            if (students.Contains(email, StringComparer.OrdinalIgnoreCase) && !isAdmin)
            {
                _logger.LogWarning("Unauthorized OTP request. Email: {Email}", email);
                var forbiddenResp = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                if (!string.IsNullOrEmpty(origin))
                {
                    forbiddenResp.Headers.Add("Access-Control-Allow-Origin", origin);
                    forbiddenResp.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                await forbiddenResp.WriteAsJsonAsync(new { message = "User not found" });
                return forbiddenResp;
            }

            try
            {
                // Generate OTP
                var otpCode = _otpService.GenerateOtp(email);
                _logger.LogInformation("Generated OTP for {Email}", email);

                // Create and send email
                var emailMessage = await _emailContentService.CreateOtpEmailAsync(email, otpCode);
                var emailSent = await _emailSender.SendEmailAsync(emailMessage);

                if (!emailSent)
                {
                    _logger.LogError("Failed to send OTP email to {Email}", email);
                    var errorResp = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                    if (!string.IsNullOrEmpty(origin))
                    {
                        errorResp.Headers.Add("Access-Control-Allow-Origin", origin);
                        errorResp.Headers.Add("Access-Control-Allow-Credentials", "true");
                    }
                    await errorResp.WriteAsJsonAsync(new { message = "Failed to send email" });
                    return errorResp;
                }

                _logger.LogInformation("OTP email sent successfully to {Email}", email);

                var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
                if (!string.IsNullOrEmpty(origin))
                {
                    response.Headers.Add("Access-Control-Allow-Origin", origin);
                    response.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                await response.WriteAsJsonAsync(new { message = "OTP sent successfully" });
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending OTP to {Email}", email);
                var errorResp = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
                if (!string.IsNullOrEmpty(origin))
                {
                    errorResp.Headers.Add("Access-Control-Allow-Origin", origin);
                    errorResp.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                await errorResp.WriteAsJsonAsync(new { message = "An error occurred while sending OTP" });
                return errorResp;
            }
        }

        [Function("VerifyOtp")]
        [OpenApiOperation(operationId: "VerifyOtp", tags: new[] { "Auth" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(VerifyOtpRequest), Required = true, Description = "Email and OTP code for verification")]
        public async Task<HttpResponseData> VerifyOtp(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", "options", Route = "auth/otp/verify")] HttpRequestData req)
        {
            _logger.LogInformation("VerifyOtp function triggered.");

            var origin = req.Headers.TryGetValues("Origin", out var origins) ? origins.FirstOrDefault() : null;
            _logger.LogDebug("Request origin: {Origin}", origin ?? "None");

            // Handle preflight request
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

            // Read POST body
            var body = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<VerifyOtpRequest>(body);
            
            if (data is null || string.IsNullOrWhiteSpace(data.Email) || string.IsNullOrWhiteSpace(data.Code))
            {
                _logger.LogWarning("VerifyOtp request missing or invalid email or code");
                var badReq = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                if (!string.IsNullOrEmpty(origin))
                {
                    badReq.Headers.Add("Access-Control-Allow-Origin", origin);
                    badReq.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                await badReq.WriteAsJsonAsync(new { message = "Email and code are required" });
                return badReq;
            }

            var email = data.Email.Trim().ToLowerInvariant();
            var code = data.Code.Trim();

            // Validate OTP
            var isValid = _otpService.ValidateOtp(email, code);

            if (!isValid)
            {
                _logger.LogWarning("OTP validation failed for {Email}", email);
                var unauthorizedResp = req.CreateResponse(System.Net.HttpStatusCode.Unauthorized);
                if (!string.IsNullOrEmpty(origin))
                {
                    unauthorizedResp.Headers.Add("Access-Control-Allow-Origin", origin);
                    unauthorizedResp.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                await unauthorizedResp.WriteAsJsonAsync(new { message = "Invalid or expired OTP code" });
                return unauthorizedResp;
            }

            // Check if user is in whitelist or is admin
            var whitelist = await _whitelistRepository.GetStudentWhitelistAsync();
            var students = whitelist?.StudentEmails ?? new List<string>();
            var adminEmailsEnv = Environment.GetEnvironmentVariable("AdminEmails") ?? "";
            var adminEmails = adminEmailsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            bool isAdmin = adminEmails.Contains(email, StringComparer.OrdinalIgnoreCase);

            if (students.Contains(email, StringComparer.OrdinalIgnoreCase) && !isAdmin)
            {
                _logger.LogWarning("User not in whitelist after OTP validation. Email: {Email}", email);
                var forbiddenResp = req.CreateResponse(System.Net.HttpStatusCode.Forbidden);
                if (!string.IsNullOrEmpty(origin))
                {
                    forbiddenResp.Headers.Add("Access-Control-Allow-Origin", origin);
                    forbiddenResp.Headers.Add("Access-Control-Allow-Credentials", "true");
                }
                await forbiddenResp.WriteAsJsonAsync(new { message = "User not found" });
                return forbiddenResp;
            }

            // Remove OTP after successful validation
            _otpService.RemoveOtp(email);

            // Generate JWT token
            var token = GenerateJwtToken(email, isAdmin);
            _logger.LogInformation("OTP verified and JWT generated for {Email}, Role: {Role}", email, isAdmin ? "Admin" : "Student");

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            if (!string.IsNullOrEmpty(origin))
            {
                response.Headers.Add("Access-Control-Allow-Origin", origin);
                response.Headers.Add("Access-Control-Allow-Credentials", "true");
            }

            // Set HttpOnly cookie
            response.Headers.Add("Set-Cookie",
                $"token={token}; HttpOnly; SameSite=None; Secure; Path=/; Max-Age=86400");

            await response.WriteAsJsonAsync(new
            {
                email = email,
                role = isAdmin ? "Admin" : "Student"
            });

            return response;
        }

        public class LoginRequest
        {
            public required string IdToken { get; set; }
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
    }
}