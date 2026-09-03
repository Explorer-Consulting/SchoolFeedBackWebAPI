using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ApplicationEventWorkers.SelfOptIn;

/*
 * Concrete JWT implementation of IOptInTokenService
 * Builds claims only for the link (no user identity):
 * purpose="optin" (guards against misuse),
 * Returns the compact JWT string, which can put in URLs.
 * 
 * Uses TokenValidationParameters to enforce signature, issuer, audience, and lifetime
 *      (with small ClockSkew)
 * 
 * Returns a structured result
 *      (and maps common failures to Error: expired, bad_signature, wrong_audience, etc.)
 * 
 * note: This token is not a login token;
 *      it carries no user claims. It’s stateless (no DB lookup needed)
 *      and revocable by changing the tag or waiting for expiry
 */

public sealed class OptInJwtService : IOptInTokenService
{
    private readonly SelfOptInJwtOptions _opt;
    private readonly JwtSecurityTokenHandler _handler = new();

    private SymmetricSecurityKey SigningKey =>
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opt.SigningKey));

    public OptInJwtService(IOptions<SelfOptInJwtOptions> opt)
    {
        _opt = opt.Value ?? throw new ArgumentNullException(nameof(opt));
        _handler.MapInboundClaims = false;
        if (string.IsNullOrWhiteSpace(_opt.SigningKey) || _opt.SigningKey.Length < 32)
            throw new InvalidOperationException("SelfOptInJwtOptions.SigningKey must be at least 32 characters.");
    }
    
    public string CreateToken(Guid questionnaireId, string tag, DateTimeOffset expiresAtUtc)
    {
        var claims = new[]
        {
            new Claim("purpose", "optin"),
            new Claim("tid", questionnaireId.ToString("D")), // GUID has hyphens 
            new Claim("tag", tag ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var creds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var desc = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = now.AddMinutes(-1),
            Expires   = expiresAtUtc.UtcDateTime,
            Issuer    = _opt.Issuer,
            Audience  = _opt.Audience,
            SigningCredentials = creds
        };

        var token = _handler.CreateToken(desc);
        return _handler.WriteToken(token);
    }
    
    public string CreateTokenWithEmail(Guid questionnaireId, string tag, string email, DateTimeOffset expiresAtUtc)
    {
        var claims = new[]
        {
            new Claim("purpose", "optin"),
            new Claim("tid", questionnaireId.ToString("D")), // GUID has hyphens 
            new Claim("tag", tag ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("email", email)
        };

        var creds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);
        var now = DateTime.UtcNow;

        var desc = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            NotBefore = now.AddMinutes(-1),
            Expires   = expiresAtUtc.UtcDateTime,
            Issuer    = _opt.Issuer,
            Audience  = _opt.Audience,
            SigningCredentials = creds
        };

        var token = _handler.CreateToken(desc);
        return _handler.WriteToken(token);
    }

    public OptInTokenValidationResult Validate(string token, DateTimeOffset nowUtc)
    {
        var tvp = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = SigningKey,
            ValidateIssuer = true,
            ValidIssuer = _opt.Issuer,
            ValidateAudience = true,
            ValidAudience = _opt.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };

        try
        {
            var principal = _handler.ValidateToken(token, tvp, out var st);

            if (st is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
                return new(false, default, string.Empty, null, "bad_algorithm");

            var purpose = principal.FindFirst("purpose")?.Value;
            if (!string.Equals(purpose, "optin", StringComparison.Ordinal))
                return new(false, default, string.Empty, null, "wrong_purpose");

            var tidText = principal.FindFirst("tid")?.Value
                        ?? principal.FindFirst("qid")?.Value;
            if (!Guid.TryParse(tidText, out var tid))
                return new(false, default, "", null, "bad_tid");

            var tag = principal.FindFirst("tag")?.Value ?? string.Empty;

            var expClaim = principal.FindFirst("exp")?.Value;
            DateTimeOffset? exp = null;
            if (long.TryParse(expClaim, out var expSec))
                exp = DateTimeOffset.FromUnixTimeSeconds(expSec);

            return new(true, tid, tag, exp, null);
        }
        catch (SecurityTokenExpiredException)          { return new(false, default, "", null, "expired"); }
        catch (SecurityTokenInvalidAudienceException)  { return new(false, default, "", null, "wrong_audience"); }
        catch (SecurityTokenInvalidIssuerException)    { return new(false, default, "", null, "wrong_issuer"); }
        catch (SecurityTokenInvalidSignatureException) { return new(false, default, "", null, "bad_signature"); }
        catch                                          { return new(false, default, "", null, "invalid_token"); }
    }
}
