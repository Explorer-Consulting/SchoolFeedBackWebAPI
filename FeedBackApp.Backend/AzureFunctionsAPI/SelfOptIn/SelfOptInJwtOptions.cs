namespace ApplicationEventWorkers.SelfOptIn;

/*
 * How the Self opt-in JWT looks like
 * The enabled tag shows if a questionaire can be accessed through the self opt-in service
 * Issuer and Audience fields will eventually append to the URL tag (SelfOptIn tag)
 */

public sealed class SelfOptInJwtOptions
{
    // No default value
    public bool Enabled { get; set; }
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public string SigningKey { get; set; } = "";
    public int TokenTtlMinutes { get; set; } 
}