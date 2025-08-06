using Infisical.Sdk;

namespace TRENZ.Extensions.Infisical.Tests;

public static class InfisicalSecretsRepositoryTest
{
    private static InfisicalConfigurationOptions GenerateCompleteConfig() => new()
    {
        UserAgent = "userAgent",
        AccessToken = "accessToken",
        CacheTtl = 1234L,
        ClientId = "clientId",
        ClientSecret = "clientSecret",
        EnvironmentName = "Production",
        LoadTimeout = 5678L,
        PollingInterval = 9012L,
        ProjectId = "projectId",
        SiteUrl = "https://siteUrl",
    };

    [Test]
    public static void TestConstructorHappyPath()
    {
        Assert.DoesNotThrow(() =>
        {
            _ = new InfisicalSecretsRepository(null, GenerateCompleteConfig());
        });
    }

    [Test]
    public static void TestConstructorDoesntThrowWithTrailingSlash()
    {
        Assert.DoesNotThrow(() =>
        {
            var config = GenerateCompleteConfig();
            config.SiteUrl = "https://siteUrl/";
            _ = new InfisicalSecretsRepository(null, config);
        });
    }

    [Test]
    public static void TestConstructorDoesntThrowIfUserAgentIsNull()
    {
        Assert.DoesNotThrow(() =>
        {
            var config = GenerateCompleteConfig();
            config.UserAgent = null;
            _ = new InfisicalSecretsRepository(null, config);
        });
    }

    [Test]
    public static void TestConstructorThrowsIfSiteUrlIsEmpty()
    {
        var e = Assert.Throws<InfisicalException>(() =>
        {
            var config = GenerateCompleteConfig();
            config.SiteUrl = string.Empty;
            _ = new InfisicalSecretsRepository(null, config);
        });

        Assert.That(e.Message, Is.EqualTo("SiteUrl is not set."));
    }

    [Test]
    public static void TestConstructorThrowsIfSiteUrlDoesNotUseHttps()
    {
        var e = Assert.Throws<InfisicalException>(() =>
        {
            var config = GenerateCompleteConfig();
            config.SiteUrl = "httpsiteUrl";
            _ = new InfisicalSecretsRepository(null, config);
        });

        Assert.That(e.Message, Is.EqualTo("SiteUrl must use HTTPS scheme"));
    }
}
