namespace GameLeaderboard.Architecture.Tests;

using System.Xml.Linq;

public sealed class ProjectReferenceRuleTests
{
    public static TheoryData<string, string[]>
        ProjectReferenceCases =>
        new()
        {
            {
                "GameLeaderboard.Domain",
                []
            },
            {
                "GameLeaderboard.Application",
                ["GameLeaderboard.Domain"]
            },
            {
                "GameLeaderboard.Infrastructure",
                [
                    "GameLeaderboard.Application",
                    "GameLeaderboard.Domain",
                ]
            },
            {
                "GameLeaderboard.Api",
                [
                    "GameLeaderboard.Application",
                    "GameLeaderboard.Infrastructure",
                ]
            },
        };

    [Theory]
    [MemberData(nameof(ProjectReferenceCases))]
    public void ProjectReferences_MustMatchArchitecture(
        string projectName,
        string[] expectedReferences)
    {
        var actualReferences =
            GetProjectReferences(projectName)
                .Order(StringComparer.Ordinal)
                .ToArray();

        var expected =
            expectedReferences
                .Order(StringComparer.Ordinal)
                .ToArray();

        Assert.Equal(expected, actualReferences);
    }

    private static IReadOnlyList<string>
        GetProjectReferences(string projectName)
    {
        var solutionRoot = FindSolutionRoot();

        var projectFile =
            FindProjectFile(
                solutionRoot,
                projectName);

        var document =
            XDocument.Load(projectFile);

        return document
            .Descendants()
            .Where(element =>
                element.Name.LocalName ==
                "ProjectReference")
            .Select(element =>
                element.Attribute("Include")?.Value)
            .Where(include =>
                !string.IsNullOrWhiteSpace(include))
            .Select(include =>
                GetReferencedProjectName(include!))
            .ToArray();
    }

    private static string FindSolutionRoot()
    {
        DirectoryInfo? directory =
            new(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var containsSolution =
                directory
                    .EnumerateFiles("*.sln")
                    .Any()
                || directory
                    .EnumerateFiles("*.slnx")
                    .Any();

            if (containsSolution)
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException(
            "Could not find the solution root directory.");
    }

    private static string FindProjectFile(
        string solutionRoot,
        string projectName)
    {
        var expectedFileName =
            $"{projectName}.csproj";

        var matches =
            Directory
                .EnumerateFiles(
                    solutionRoot,
                    expectedFileName,
                    SearchOption.AllDirectories)
                .Where(path =>
                    !IsBuildOutputPath(
                        solutionRoot,
                        path))
                .ToArray();

        return matches.Length switch
        {
            1 => matches[0],

            0 => throw new InvalidOperationException(
                $"Could not find project '{projectName}'."),

            _ => throw new InvalidOperationException(
                $"Found multiple project files named " +
                $"'{expectedFileName}':{Environment.NewLine}" +
                string.Join(
                    Environment.NewLine,
                    matches)),
        };
    }

    private static bool IsBuildOutputPath(
        string solutionRoot,
        string path)
    {
        var relativePath =
            Path.GetRelativePath(
                solutionRoot,
                path);

        var segments =
            relativePath.Split(
                [
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar,
                ],
                StringSplitOptions.RemoveEmptyEntries);

        return segments.Any(segment =>
            segment is "bin" or "obj");
    }

    private static string GetReferencedProjectName(
        string projectReference)
    {
        var normalizedPath =
            projectReference
                .Replace(
                    '\\',
                    Path.DirectorySeparatorChar)
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar);

        return Path.GetFileNameWithoutExtension(
            normalizedPath);
    }
}