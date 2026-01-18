using System.Collections.Frozen;
using Infisical.Sdk;
using Moq;

namespace TRENZ.Extensions.Infisical.Tests;

public static class DoubleUnderscoreToColonMappingClientWrapperTest
{
    [TestCase("Foo", "Foo")]
    [TestCase("Foo:Bar", "Foo:Bar")]
    [TestCase("Foo__Bar", "Foo:Bar")]
    public static void TestMapsCorrectly(string inputKey, string outputKey)
    {
        var innerMock = new Mock<ISecretsRepository>();

        innerMock
            .Setup(c => c.GetAllSecrets())
            .Returns(new Dictionary<string, Secret> { { inputKey, new() } })
            .Verifiable();

        var mapper = new DoubleUnderscoreToColonMappingSecretsRepository(innerMock.Object);

        var all = mapper.GetAllSecrets();

        Assert.That(all, Is.Not.Null);
        Assert.That(all, Has.Count.AtLeast(1));
        Assert.That(all, Does.ContainKey(inputKey));
        Assert.That(all, Does.ContainKey(outputKey));
    }

    [Test]
    public static void TestMapsWithFrozenDictionary()
    {
        var innerMock = new Mock<ISecretsRepository>();

        innerMock
            .Setup(c => c.GetAllSecrets())
            .Returns(new Dictionary<string, Secret> { { "Foo", new() } }.ToFrozenDictionary())
            .Verifiable();

        var mapper = new DoubleUnderscoreToColonMappingSecretsRepository(innerMock.Object);

        var all = mapper.GetAllSecrets();

        Assert.That(all, Is.Not.Null);
    }
}
