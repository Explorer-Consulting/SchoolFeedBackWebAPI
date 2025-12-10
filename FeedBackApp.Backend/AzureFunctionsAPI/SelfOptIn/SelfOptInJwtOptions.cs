namespace ApplicationEventWorkers.SelfOptIn;

/*
 * How the Self opt-in JWT looks like
 * The enabled tag shows if a questionaire can be accessed through the self opt-in service
 * Issuer and Audience fields will eventually append to the URL tag (SelfOptIn tag)
 */

public sealed class SelfOptInJwtOptions
{
    public bool Enabled { get; set; } = true;
    public string Issuer { get; set; } = "feedback-app.optin";   // source
    public string Audience { get; set; } = "feedback-app.optin"; // destination
    public string SigningKey { get; set; } = "f9W8zP4vQjL7mX2sTdNz6YrUaV0KeH1Cb"; // used jwt secret key
    public int TokenTtlMinutes { get; set; } = 7 * 24 * 60; // 7 days
}