/// <summary>
/// Type-safe app settings for services in user module.
/// <see cref="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-8.0"/>
/// for more on this pattern.
/// </summary>
public class UserServiceOptions
{
    public const string CustomAWSSection = "AWS-Custom";

    public string ProfileName { get; set; } = String.Empty;
    public string S3BucketName { get; set; } = String.Empty;
    public string Region { get; set; } = String.Empty;
}