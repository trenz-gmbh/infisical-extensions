using Infisical.Sdk;

namespace TRENZ.Extensions.Infisical.Tests;

public static class InfisicalSecretsRepositoryTest
{
    private static InfisicalConfigurationOptions CompleteConfig => new()
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
            _ = new InfisicalSecretsRepository(null, CompleteConfig);
        });
    }

    [Test]
    public static void TestConstructorDoesntThrowWithTrailingSlash()
    {
        Assert.DoesNotThrow(() =>
        {
            var cloned = CompleteConfig.Clone();
            cloned.SiteUrl = "https://siteUrl/";
            _ = new InfisicalSecretsRepository(null, cloned);
        });
    }

    [Test]
    public static void TestConstructorDoesntThrowIfUserAgentIsNull()
    {
        Assert.DoesNotThrow(() =>
        {
            var cloned = CompleteConfig.Clone();
            cloned.UserAgent = null;
            _ = new InfisicalSecretsRepository(null, cloned);
        });
    }

    [Test]
    public static void TestConstructorThrowsIfSiteUrlIsEmpty()
    {
        var e = Assert.Throws<InfisicalException>(() =>
        {
            var cloned = CompleteConfig.Clone();
            cloned.SiteUrl = string.Empty;
            _ = new InfisicalSecretsRepository(null, cloned);
        });

        Assert.That(e.Message, Is.EqualTo("SiteUrl is not set."));
    }

    [Test]
    public static void TestConstructorThrowsIfSiteUrlDoesNotUseHttps()
    {
        var e = Assert.Throws<InfisicalException>(() =>
        {
            var cloned = CompleteConfig.Clone();
            cloned.SiteUrl = "httpsiteUrl";
            _ = new InfisicalSecretsRepository(null, cloned);
        });

        Assert.That(e.Message, Is.EqualTo("SiteUrl must use HTTPS scheme"));
    }
}
