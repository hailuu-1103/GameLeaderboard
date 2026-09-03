namespace GameLeaderboard.Architecture.Tests;

using NetArchTest.Rules;

public class NamespaceRuleTests
{
    [Fact]
    public void Controllers_MustResideInControllersNamespace()
    {
        var result =
            Types.InAssembly(Assemblies.Api)
                .That()
                .HaveNameEndingWith("Controller")
                .Should()
                .ResideInNamespace(
                    "GameLeaderboard.Api.Controllers")
                .GetResult();

        Assert.True(result.IsSuccessful);
    }
}