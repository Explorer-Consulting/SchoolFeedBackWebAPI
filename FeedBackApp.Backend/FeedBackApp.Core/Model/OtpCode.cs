namespace FeedBackApp.Core.Model;

/// <summary>
/// Represents an OTP (One-Time Password) code with expiration information.
/// </summary>
public class OtpCode
{
    /// <summary>
    /// The email address associated with this OTP.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// The OTP code (6-digit numeric string).
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// When this OTP was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this OTP expires.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Number of verification attempts made with this OTP.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Maximum number of verification attempts allowed.
    /// </summary>
    public const int MaxAttempts = 5;

    /// <summary>
    /// Checks if the OTP is still valid (not expired and not exceeded max attempts).
    /// </summary>
    public bool IsValid => DateTime.UtcNow <= ExpiresAt && Attempts < MaxAttempts;
}

