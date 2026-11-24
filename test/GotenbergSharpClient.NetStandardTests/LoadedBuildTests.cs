using System.Reflection;
using System.Runtime.Versioning;

namespace GotenbergSharpClient.NetStandardTests;

[TestFixture]
public class LoadedBuildTests
{
    [Test]
    public void TheTestsRunTheNetStandard20BuildOfTheClient() =>
        typeof(Gotenberg.Sharp.API.Client.GotenbergSharpClient).Assembly
            .GetCustomAttribute<TargetFrameworkAttribute>()!.FrameworkName
            .Should().Be(".NETStandard,Version=v2.0");
}
