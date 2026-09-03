namespace GameLeaderboard.Architecture.Tests;

using NetArchTest.Rules;
using Xunit;

public sealed class DependencyRuleTests
{
    [Fact]
    public void Domain_MustNotDependOnOuterLayers()
    {
        var result =
            Types.InAssembly(Assemblies.Domain)
                .ShouldNot()
                .HaveDependencyOnAny(
                    Assemblies.APPLICATION_NAMESPACE,
                    Assemblies.INFRASTRUCTURE_NAMESPACE,
                    Assemblies.API_NAMESPACE,
                    "Microsoft.AspNetCore",
                    "Microsoft.EntityFrameworkCore")
                .GetResult();

        AssertRuleSucceeded(result);
    }

    [Fact]
    public void Application_MustNotDependOnOuterLayers()
    {
        var result =
            Types.InAssembly(Assemblies.Application)
                .ShouldNot()
                .HaveDependencyOnAny(
                    Assemblies.INFRASTRUCTURE_NAMESPACE,
                    Assemblies.API_NAMESPACE,
                    "Microsoft.AspNetCore",
                    "Microsoft.EntityFrameworkCore")
                .GetResult();

        AssertRuleSucceeded(result);
    }

    [Fact]
    public void Infrastructure_MustNotDependOnApi()
    {
        var result =
            Types.InAssembly(Assemblies.Infrastructure)
                .ShouldNot()
                .HaveDependencyOn(
                    Assemblies.API_NAMESPACE)
                .GetResult();

        AssertRuleSucceeded(result);
    }

    [Fact]
    public void Controllers_MustNotDependOnInfrastructure()
    {
        var result =
            Types.InAssembly(Assemblies.Api)
                .That()
                .HaveNameEndingWith("Controller")
                .ShouldNot()
                .HaveDependencyOn(
                    Assemblies.INFRASTRUCTURE_NAMESPACE)
                .GetResult();

        AssertRuleSucceeded(result);
    }





    private static void AssertRuleSucceeded(TestResult result)
    {
        var failingTypes =
            result.FailingTypes is null
                ? "No failing types were reported."
                : string.Join(
                    Environment.NewLine,
                    result.FailingTypes.Select(type => type.FullName));

        Assert.True(
            result.IsSuccessful,
            $"Architecture violations:{Environment.NewLine}{failingTypes}");
    }
}