using System.Collections.Frozen;
using Infisical.Sdk;
using Moq;

namespace TRENZ.Extensions.Infisical.Tests;

public static class UnderscoreToColonMappingClientWrapperTest
{
    [TestCase("Foo", "Foo")]
    [TestCase("Foo:Bar", "Foo:Bar")]
    [TestCase("Foo__Bar", "Foo:Bar")]
    public static void TestMapsCorrectly(string inputKey, string outputKey)
    {
        var innerMock = new Mock<IInfisicalClientWrapper>();

        innerMock
            .Setup(c => c.GetAll())
            .Returns(new Dictionary<string, SecretElement> { { inputKey, new() } })
            .Verifiable();

        var mapper = new UnderscoreToColonMappingInfisicalClientWrapper(innerMock.Object);

        var all = mapper.GetAll();

        Assert.That(all, Is.Not.Null);
        Assert.That(all, Has.Count.AtLeast(1));
        Assert.That(all, Does.ContainKey(inputKey));
        Assert.That(all, Does.ContainKey(outputKey));
    }

    [Test]
    public static void TestMapsWithFrozenDictionary()
    {
        var innerMock = new Mock<IInfisicalClientWrapper>();

        innerMock
            .Setup(c => c.GetAll())
            .Returns(new Dictionary<string, SecretElement> { { "Foo", new() } }.ToFrozenDictionary())
            .Verifiable();

        var mapper = new UnderscoreToColonMappingInfisicalClientWrapper(innerMock.Object);

        var all = mapper.GetAll();

        Assert.That(all, Is.Not.Null);
    }
}
