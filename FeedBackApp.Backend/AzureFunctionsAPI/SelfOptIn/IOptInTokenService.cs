using System;

namespace ApplicationEventWorkers.SelfOptIn;

public interface IOptInTokenService
{
    string CreateToken(Guid questionnaireId, string tag, DateTimeOffset expiresAtUtc);
    OptInTokenValidationResult Validate(string token, DateTimeOffset nowUtc);
}

// fileba dto
public sealed record OptInTokenValidationResult(
    bool IsValid,
    Guid QuestionnaireId,
    string Tag,
    DateTimeOffset? ExpiresAtUtc,
    string? Error
);