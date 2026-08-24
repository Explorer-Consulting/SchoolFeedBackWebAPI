using Application.Services.Interfaces;
using FeedBackApp.Backend.Infrastructure.Configuration;
using FeedBackApp.Core.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace Application.Services;

/// <summary>
/// In-memory implementation of OTP service for generating and validating OTP codes.
/// OTPs are stored in memory and automatically expire after a configured duration.
/// </summary>
public class OtpService : IOtpService
{
    private readonly ILogger<OtpService> _logger;
    private readonly ConcurrentDictionary<string, OtpCode> _otpStore;
    private readonly int _otpExpirationMinutes;
    private readonly Random _random;

    public OtpService(ILogger<OtpService> logger, IOptions<OtpOptions> otpOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _otpStore = new ConcurrentDictionary<string, OtpCode>(StringComparer.OrdinalIgnoreCase);
        _otpExpirationMinutes = otpOptions.Value.ExpirationMinutes; // Default 10 minutes
        _random = new Random();

        // Start background task to clean up expired OTPs
        _ = Task.Run(CleanupExpiredOtps);
    }

    /// <summary>
    /// Generates a new 6-digit OTP code for the specified email address.
    /// If an OTP already exists for this email, it will be replaced.
    /// </summary>
    public string GenerateOtp(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException("Email cannot be null or empty", nameof(email));
        }

        var code = _random.Next(100000, 999999).ToString();
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(_otpExpirationMinutes);

        var otpCode = new OtpCode
        {
            Email = email,
            Code = code,
            CreatedAt = now,
            ExpiresAt = expiresAt,
            Attempts = 0
        };

        _otpStore.AddOrUpdate(email, otpCode, (key, oldValue) => otpCode);

        _logger.LogInformation(
            "Generated OTP for {Email}. Expires at {ExpiresAt}",
            email,
            expiresAt);

        return code;
    }

    /// <summary>
    /// Validates an OTP code for the specified email address.
    /// Increments the attempt counter on each validation attempt.
    /// </summary>
    public bool ValidateOtp(string email, string code)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning("Validation attempt with null or empty email or code");
            return false;
        }

        if (!_otpStore.TryGetValue(email, out var otpCode))
        {
            _logger.LogWarning("OTP validation failed: No OTP found for {Email}", email);
            return false;
        }

        // Check if OTP is expired
        if (DateTime.UtcNow > otpCode.ExpiresAt)
        {
            _logger.LogWarning("OTP validation failed: OTP expired for {Email}", email);
            _otpStore.TryRemove(email, out _);
            return false;
        }

        // Check if max attempts exceeded
        if (otpCode.Attempts >= OtpCode.MaxAttempts)
        {
            _logger.LogWarning("OTP validation failed: Max attempts exceeded for {Email}", email);
            _otpStore.TryRemove(email, out _);
            return false;
        }

        // Increment attempts
        otpCode.Attempts++;

        // Validate code
        if (otpCode.Code != code)
        {
            _logger.LogWarning(
                "OTP validation failed: Invalid code for {Email}. Attempt {Attempt}/{MaxAttempts}",
                email,
                otpCode.Attempts,
                OtpCode.MaxAttempts);
            return false;
        }

        _logger.LogInformation("OTP validation successful for {Email}", email);
        return true;
    }

    /// <summary>
    /// Removes an OTP code for the specified email address.
    /// Typically called after successful validation.
    /// </summary>
    public void RemoveOtp(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        if (_otpStore.TryRemove(email, out _))
        {
            _logger.LogInformation("Removed OTP for {Email}", email);
        }
    }

    /// <summary>
    /// Background task to periodically clean up expired OTPs from memory.
    /// </summary>
    private async Task CleanupExpiredOtps()
    {
        while (true)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(5)); // Run every 5 minutes

                var now = DateTime.UtcNow;
                var expiredKeys = _otpStore
                    .Where(kvp => now > kvp.Value.ExpiresAt || kvp.Value.Attempts >= OtpCode.MaxAttempts)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in expiredKeys)
                {
                    _otpStore.TryRemove(key, out _);
                }

                if (expiredKeys.Any())
                {
                    _logger.LogDebug("Cleaned up {Count} expired OTPs", expiredKeys.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OTP cleanup");
            }
        }
    }
}

