using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;

namespace TRENZ.Extensions.Infisical.Tests;

public class ConfigureBuilderExtensionsTest
{
    private static IConfigurationRoot MockConfiguration(
        string? clientId = null,
        string? clientSecret = null,
        string? siteUrl = null,
        string? projectId = null,
        string? accessToken = null,
        string? cacheTtl = null,
        string? userAgent = null,
        string? pollingInterval = null)
    {
        var dict = new Dictionary<string, string?>
        {
            { "Infisical:ClientId", clientId },
            { "Infisical:ClientSecret", clientSecret },
            { "Infisical:SiteUrl", siteUrl },
            { "Infisical:ProjectId", projectId },
            { "Infisical:AccessToken", accessToken },
            { "Infisical:CacheTtl", cacheTtl },
            { "Infisical:UserAgent", userAgent },
            { "Infisical:PollingInterval", pollingInterval },
        };

        return new ConfigurationBuilder()
            .AddInMemoryCollection(dict)
            .Build();
    }

    [Test]
    public void TestAddInfisicalCallsConfigure()
    {
        var args = new List<InfisicalConfigurationSource>();
        var builderMock = new Mock<IConfigurationBuilder>();
        builderMock
            .Setup(b => b.Add(Capture.In(args)))
            .Returns(builderMock.Object);

        builderMock.Setup(b => b.Build()).Returns(MockConfiguration());

        var called = false;
        var configureCallback = (InfisicalConfigurationOptions options) => { called = true; };

        builderMock.Object.AddInfisical(configureCallback);

        Assert.Multiple(() =>
        {
            Assert.That(called, Is.True);
            Assert.That(args, Is.Not.Empty);
        });
    }

    [Test]
    public void TestAllOptionsAreReadFromConfiguration()
    {
        var tempConfig = MockConfiguration(
            clientId: "clientId",
            clientSecret: "clientSecret",
            siteUrl: "siteUrl",
            projectId: "projectId",
            accessToken: "accessToken",
            cacheTtl: "1000",
            userAgent: "userAgent",
            pollingInterval: "1000"
        );

        var builderMock = new Mock<IConfigurationBuilder>();
        builderMock.Setup(b => b.Build()).Returns(tempConfig);

        InfisicalConfigurationOptions? options = null;
        builderMock.Object.AddInfisical(o =>
        {
            options = o;
        });

        Assert.That(options, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(options!.EnvironmentName, Is.EqualTo("Development"));
            Assert.That(options.ClientId, Is.EqualTo("clientId"));
            Assert.That(options.ClientSecret, Is.EqualTo("clientSecret"));
            Assert.That(options.SiteUrl, Is.EqualTo("siteUrl"));
            Assert.That(options.ProjectId, Is.EqualTo("projectId"));
            Assert.That(options.AccessToken, Is.EqualTo("accessToken"));
            Assert.That(options.CacheTtl, Is.EqualTo(1000));
            Assert.That(options.UserAgent, Is.EqualTo("userAgent"));
            Assert.That(options.PollingInterval, Is.EqualTo(1000));
        });
    }

    [Test]
    public void TestEnvironmentNameIsReadFromIEnvironment()
    {
        var tempConfig = MockConfiguration();

        var environmentMock = new Mock<IHostEnvironment>();
        environmentMock.Setup(e => e.EnvironmentName).Returns("Test");

        var builderMock = new Mock<IConfigurationBuilder>();
        builderMock.Setup(b => b.Build()).Returns(tempConfig);

        InfisicalConfigurationOptions? options = null;
        builderMock.Object.AddInfisical(environmentMock.Object, o =>
        {
            options = o;
        });

        Assert.That(options, Is.Not.Null);
        Assert.That(options!.EnvironmentName, Is.EqualTo("Test"));
    }
}
