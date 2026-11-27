namespace CareEMR.Api.Auth;

// Bound from the "Jwt" config section.
public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    // SECRET - comes from User Secrets / environment variables, never from committed files.
    public string SigningKey { get; set; } = string.Empty;

    public int ExpiryMinutes { get; set; } = 60;
}

// DEMO ONLY: a single hard-wired login so the API can be tried out.
// A real system would store users (with hashed passwords) or use an identity provider.
public class DemoUserOptions
{
    public const string SectionName = "DemoUser";

    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
