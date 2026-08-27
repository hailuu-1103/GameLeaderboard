using GameLeaderboard.API.ErrorHandling;
using GameLeaderboard.Application.Abstractions.Persistence;
using GameLeaderboard.Application.Leaderboards.GetPlayer;
using GameLeaderboard.Application.Leaderboards.GetTop;
using GameLeaderboard.Application.Scores.SubmitScore;
using GameLeaderboard.Domain.Scores;
using GameLeaderboard.Infrastructure.Persistence.InMemory;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions
        .RespectRequiredConstructorParameters = true;

    options.JsonSerializerOptions
        .RespectNullableAnnotations = true;
});
builder.Services.AddOpenApi();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance ??=
            context.HttpContext.Request.Path.ToString();

        context.ProblemDetails.Extensions["traceId"] =
            context.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSingleton(
    TimeProvider.System);

builder.Services.AddSingleton<
    ScoreSubmissionPolicy>();

builder.Services.AddSingleton<
    InMemoryLeaderboardStore>();

builder.Services.AddSingleton<
    ILeaderboardRepository,
    InMemoryLeaderboardRepository>();

builder.Services.AddSingleton<
    ISeasonRepository,
    InMemorySeasonRepository>();

builder.Services.AddSingleton<
    IPlayerScoreRepository,
    InMemoryPlayerScoreRepository>();

builder.Services.AddSingleton<
    ILeaderboardQueries,
    InMemoryLeaderboardQueries>();

builder.Services.AddSingleton<
    IUnitOfWork,
    InMemoryUnitOfWork>();

builder.Services.AddScoped<
    SubmitScoreHandler>();

builder.Services.AddScoped<
    GetTopPlayerHandler>();

builder.Services.AddScoped<
    GetPlayerHandler>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program
{
}