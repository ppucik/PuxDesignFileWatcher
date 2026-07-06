using Microsoft.Extensions.DependencyInjection;
using PuxDesignFileWatcher.Application.Abstractions.Cqrs;
using PuxDesignFileWatcher.Application.UseCases.AnalyzeDirectory;
using PuxDesignFileWatcher.Application.UseCases.DiffState;
using PuxDesignFileWatcher.Application.UseCases.LoadState;
using PuxDesignFileWatcher.Application.UseCases.SaveState;
using PuxDesignFileWatcher.Domain.Changes;
using PuxDesignFileWatcher.Domain.Entities;

namespace PuxDesignFileWatcher.Application;

/// <summary>
/// Application service registration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers application use-cases and CQRS handlers.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddTransient<IQueryHandler<LoadStateQuery, DirectoryManifestSnapshot?>, LoadStateQueryHandler>();
        services.AddTransient<ICommandHandler<SaveStateCommand, bool>, SaveStateCommandHandler>();
        services.AddTransient<ICommandHandler<DiffStateCommand, ManifestDiffResult>, DiffStateCommandHandler>();
        services.AddTransient<ICommandHandler<AnalyzeDirectoryCommand, DirectoryAnalysisResult>, AnalyzeDirectoryCommandHandler>();

        return services;
    }
}
