using Microsoft.AspNetCore.Http.HttpResults;
using PuxDesignFileWatcher.Application;
using PuxDesignFileWatcher.Application.Abstractions.CQRS;
using PuxDesignFileWatcher.Application.UseCases.AnalyzeDirectory;
using PuxDesignFileWatcher.Domain.Changes;
using PuxDesignFileWatcher.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.Title = "PuxDesignFileWatcher API";
});

app.UseHttpsRedirection();

app.MapPost(
    "/api/analysis",
    async Task<Results<BadRequest<string>, Ok<AnalysisResponseDto>>> (
        AnalysisRequestDto request,
        ICommandHandler<AnalyzeDirectoryCommand, DirectoryAnalysisResult> handler,
        CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(request.RootPath))
        {
            return TypedResults.BadRequest("RootPath is required.");
        }

        var result = await handler.HandleAsync(new AnalyzeDirectoryCommand(request.RootPath), cancellationToken);

        var response = new AnalysisResponseDto(
            result.RootPath,
            result.AnalyzedAtUtc,
            MapByType(result, ChangeType.New),
            MapByType(result, ChangeType.Changed),
            MapByType(result, ChangeType.Deleted));

        return TypedResults.Ok(response);
    })
.WithName("AnalyzeDirectory")
.WithSummary("Runs manual analysis for a directory path and returns detected changes.")
.WithDescription("Performs a single on-demand analysis run. No automatic filesystem watcher is used.");

app.Run();

static IReadOnlyCollection<AnalysisItemDto> MapByType(DirectoryAnalysisResult result, ChangeType type)
    => result.Changes
        .Where(change => change.ChangeType == type)
        .Select(change => new AnalysisItemDto(change.RelativePath, change.Version))
        .OrderBy(change => change.Path, StringComparer.OrdinalIgnoreCase)
        .ToArray();

public sealed record AnalysisRequestDto(string RootPath);

public sealed record AnalysisItemDto(string Path, int Version);

public sealed record AnalysisResponseDto(
    string RootPath,
    DateTimeOffset AnalyzedAtUtc,
    IReadOnlyCollection<AnalysisItemDto> New,
    IReadOnlyCollection<AnalysisItemDto> Changed,
    IReadOnlyCollection<AnalysisItemDto> Deleted);
