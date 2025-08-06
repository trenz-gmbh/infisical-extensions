namespace TRENZ.Extensions.Infisical;

public class InfisicalConfigurationOptions : ICloneable
{
    public string EnvironmentName { get; set; } = "Development";

    public string? SiteUrl { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string? ProjectId { get; set; }

    public string? AccessToken { get; set; }

    public long? CacheTtl { get; set; }

    public string? UserAgent { get; set; }

    public long? PollingInterval { get; set; }

    public long? LoadTimeout { get; set; }

    public bool? DisableDoubleUnderscoreToColonMapping { get; set; }

    object ICloneable.Clone() => Clone();

    public InfisicalConfigurationOptions Clone()
    {
        return new()
        {
            EnvironmentName = EnvironmentName,
            SiteUrl = SiteUrl,
            ClientId = ClientId,
            ClientSecret = ClientSecret,
            ProjectId = ProjectId,
            AccessToken = AccessToken,
            CacheTtl = CacheTtl,
            UserAgent = UserAgent,
            PollingInterval = PollingInterval,
            LoadTimeout = LoadTimeout,
        };
    }
}
