# Repository Guidelines

## Project Structure & Module Organization

- `GameLeaderboard.API/` contains the ASP.NET Core host. Keep HTTP endpoints in `Controllers/`, application behavior in `Services/`, and cross-cutting error responses in `ErrorHandling/`.
- `GameLeaderboard.Domain/` holds domain entities, value objects, policies, and rule violations, grouped by feature (`Leaderboard/`, `Scores/`, and `Season/`). Domain code should not depend on the API project.
- `GameLeaderboard.Api.Tests/` contains xUnit tests. Service-level tests live at the project root; HTTP contract tests and their `WebApplicationFactory` helpers live in `Integration/`.
- Request samples are in `GameLeaderboard.API/Requests/` and `GameLeaderboard.API/GameLeaderboard.API.http`. Record important domain choices in `leaderboard-domain-decisions.md`.

## Build, Test, and Development Commands

Use the .NET 10 SDK from the repository root:

```bash
dotnet restore GameLeaderboard.API.sln
dotnet build GameLeaderboard.API.sln --no-restore
dotnet test GameLeaderboard.API.sln --no-build
dotnet run --project GameLeaderboard.API
```

The run command starts the development API at `http://localhost:5114` (and, with the HTTPS profile, `https://localhost:7074`). Check formatting with `dotnet format GameLeaderboard.API.sln --verify-no-changes`. Generate coverage locally with `dotnet test --collect:"XPlat Code Coverage"`.

## Coding Style & Naming Conventions

Use four-space indentation and standard modern C# conventions. Prefer file-scoped namespaces, nullable-aware code, primary constructors where they improve clarity, and small immutable records for request/response or value types. Use `PascalCase` for types, methods, and public members; use `camelCase` for parameters, locals, and private fields. Existing classes qualify instance fields with `this.`. Keep namespaces under `GameLeaderboard.API` or `GameLeaderboard.Domain` and group new files by feature.

## Testing Guidelines

Use xUnit `[Fact]` for a single case and `[Theory]` for data-driven cases. Name tests `Member_WhenCondition_ExpectedResult`, for example `SubmitScore_WhenScoreIsHigher_UpdatesHighScore`. Follow Arrange/Act/Assert and test observable behavior. Add integration coverage when routes, validation, status codes, Problem Details, or JSON contracts change. No numeric coverage threshold is configured; cover new branches and regressions.

## Commit & Pull Request Guidelines

History follows Conventional Commit-style subjects such as `feat(leaderboard): ...`, `test(leaderboard): ...`, and `chore: ...`. Keep commits focused and subjects imperative. Pull requests should explain behavior changes, list verification commands, link relevant issues, and include sample HTTP requests/responses for API contract changes. Call out breaking route or payload changes and update request samples and domain-decision notes when applicable.

## Security & Configuration

Do not commit secrets to `appsettings*.json`. Use environment variables or .NET user secrets for local credentials. The current leaderboard service is in-memory, so data resets whenever the process restarts.
