using System;
using NUlid;

namespace ApplicationEventWorkers.SelfOptIn;

public interface IOptInTokenService
{
    string CreateToken(Ulid questionnaireId, string tag, DateTimeOffset expiresAtUtc);
    OptInTokenValidationResult Validate(string token, DateTimeOffset nowUtc);
}

// fileba dto
public sealed record OptInTokenValidationResult(
    bool IsValid,
    Ulid QuestionnaireId,
    string Tag,
    DateTimeOffset? ExpiresAtUtc,
    string? Error
);