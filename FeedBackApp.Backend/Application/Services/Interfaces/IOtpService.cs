namespace Application.Services.Interfaces;

/// <summary>
/// Service for managing OTP (One-Time Password) generation and validation.
/// </summary>
public interface IOtpService
{
    /// <summary>
    /// Generates a new OTP code for the specified email address.
    /// </summary>
    /// <param name="email">The email address to generate an OTP for.</param>
    /// <returns>The generated OTP code (6-digit string).</returns>
    string GenerateOtp(string email);

    /// <summary>
    /// Validates an OTP code for the specified email address.
    /// </summary>
    /// <param name="email">The email address associated with the OTP.</param>
    /// <param name="code">The OTP code to validate.</param>
    /// <returns>True if the OTP is valid, false otherwise.</returns>
    bool ValidateOtp(string email, string code);

    /// <summary>
    /// Removes an OTP code for the specified email address (after successful validation).
    /// </summary>
    /// <param name="email">The email address to remove the OTP for.</param>
    void RemoveOtp(string email);
}

