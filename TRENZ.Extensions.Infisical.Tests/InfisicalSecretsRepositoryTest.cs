using Infisical.Sdk;

namespace TRENZ.Extensions.Infisical.Tests;

public static class InfisicalSecretsRepositoryTest
{
    private static InfisicalConfigurationOptions GenerateCompleteOptions() => new()
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
    public static void TestCreateSettingsFromOptionsHappyPath()
    {
        var settings = InfisicalSecretsRepository.ValidateSettingsFromOptions(GenerateCompleteOptions());
        using (Assert.EnterMultipleScope())
        {
            Assert.That(settings.UserAgent, Is.EqualTo("userAgent"));
            Assert.That(settings.AccessToken, Is.EqualTo("accessToken"));
            Assert.That(settings.CacheTtl, Is.EqualTo(1234L));
            Assert.That(settings.ClientId, Is.EqualTo("clientId"));
            Assert.That(settings.ClientSecret, Is.EqualTo("clientSecret"));
            Assert.That(settings.SiteUrl, Is.EqualTo("https://siteurl"));
        }
    }

    [Test]
    public static void TestCreateSettingsFromOptionsRemovesTrailingSlash()
    {
        var options = GenerateCompleteOptions();
        options.SiteUrl = "https://siteUrl/";

        var settings = InfisicalSecretsRepository.ValidateSettingsFromOptions(options);

        Assert.That(settings.SiteUrl, Is.EqualTo("https://siteurl"));
    }

    [Test]
    public static void TestCreateSettingsFromOptionsDoesNotThrowIfUserAgentIsNull()
    {
        Assert.DoesNotThrow(() =>
        {
            var config = GenerateCompleteOptions();
            config.UserAgent = null;

            _ = InfisicalSecretsRepository.ValidateSettingsFromOptions(config);
        });
    }

    [Test]
    public static void TestConstructorThrowsIfSiteUrlIsEmpty()
    {
        var e = Assert.Throws<InfisicalException>(() =>
        {
            var config = GenerateCompleteOptions();
            config.SiteUrl = string.Empty;

            _ = InfisicalSecretsRepository.ValidateSettingsFromOptions(config);
        });

        Assert.That(e.Message, Is.EqualTo("SiteUrl is not set."));
    }

    [Test]
    public static void TestConstructorThrowsIfProjectIdIsEmpty()
    {
        var e = Assert.Throws<InfisicalException>(() =>
        {
            var config = GenerateCompleteOptions();
            config.ProjectId = string.Empty;

            _ = InfisicalSecretsRepository.CreateSettingsFromOptions(config);
        });

        Assert.That(e.Message, Is.EqualTo("ProjectId is not set."));
    }

    [Test]
    public static void TestSiteUrlAllowsHttpForLocalhost()
    {
        var config = GenerateCompleteOptions();
        config.SiteUrl = "http://localhost:8080";

        var settings = InfisicalSecretsRepository.ValidateSettingsFromOptions(config);

        Assert.That(settings.SiteUrl, Is.EqualTo("http://localhost:8080"));
    }

    [Theory]
    [TestCase("//httpsiteurl.com")]
    [TestCase("http://example.com")]
    [TestCase("ftps://example.com")]
    public static void TestConstructorThrowsIfSiteUrlDoesNotUseHttps(string siteUrl)
    {
        var e = Assert.Throws<InfisicalException>(() =>
        {
            var config = GenerateCompleteOptions();
            config.SiteUrl = siteUrl;

            _ = InfisicalSecretsRepository.ValidateSettingsFromOptions(config);
        });

        Assert.That(e.Message, Is.EqualTo("SiteUrl must use HTTPS scheme"));
    }
}
