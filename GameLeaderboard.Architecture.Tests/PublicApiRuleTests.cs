using NetArchTest.Rules;

namespace GameLeaderboard.Architecture.Tests;

public sealed class PublicApiRuleTests
{
    [Fact]
    public void PersistenceImplementations_MustNotBePublic()
    {
        var result =
            Types.InAssembly(Assemblies.Infrastructure)
                .That()
                .ResideInNamespaceStartingWith(
                    "GameLeaderboard.Infrastructure.Persistence")
                .And()
                .AreClasses()
                .ShouldNot()
                .BePublic()
                .GetResult();

        Assert.True(
            result.IsSuccessful,
            "Infrastructure persistence details must be internal.");
    }
}