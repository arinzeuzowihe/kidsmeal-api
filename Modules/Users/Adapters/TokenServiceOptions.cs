public class TokenServiceOptions
{
    public const string JwtSection = "Jwt";

    public string Key { get; set; } = String.Empty;
    public string Issuer { get; set; } = String.Empty;
    public string Audience { get; set; } = String.Empty;
    public int TokenExpiration { get; set; } = 3600;
    public int RefreshTokenExpiration { get; set; } = 86400;
    public int RefreshTokenIssuanceThreshold { get; set; } = 3600;
}