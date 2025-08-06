using System.Collections.Frozen;
using Infisical.Sdk;
using Moq;

namespace TRENZ.Extensions.Infisical.Tests;

public static class DoubleUnderscoreToColonMappingClientWrapperTest
{
    [TestCase("Foo", "Foo")]
    [TestCase("Foo:Bar", "Foo:Bar")]
    [TestCase("Foo__Bar", "Foo:Bar")]
    public static async Task TestMapsCorrectly(string inputKey, string outputKey)
    {
        var innerMock = new Mock<ISecretsRepository>();

        innerMock
            .Setup(c => c.GetAllSecretsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, SecretElement> { { inputKey, new() } })
            .Verifiable();

        var mapper = new DoubleUnderscoreToColonMappingSecretsRepository(innerMock.Object);

        var all = await mapper.GetAllSecretsAsync();

        Assert.That(all, Is.Not.Null);
        Assert.That(all, Has.Count.AtLeast(1));
        Assert.That(all, Does.ContainKey(inputKey));
        Assert.That(all, Does.ContainKey(outputKey));
    }

    [Test]
    public static async Task TestMapsWithFrozenDictionary()
    {
        var innerMock = new Mock<ISecretsRepository>();

        innerMock
            .Setup(c => c.GetAllSecretsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<string, SecretElement> { { "Foo", new() } }.ToFrozenDictionary())
            .Verifiable();

        var mapper = new DoubleUnderscoreToColonMappingSecretsRepository(innerMock.Object);

        var all = await mapper.GetAllSecretsAsync();

        Assert.That(all, Is.Not.Null);
    }
}
