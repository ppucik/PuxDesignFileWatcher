using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Infrastructure.Analysis;
using PuxDesignFileWatcher.Infrastructure.Configuration;
using PuxDesignFileWatcher.Infrastructure.Concurrency;
using PuxDesignFileWatcher.Infrastructure.FileSystem;
using PuxDesignFileWatcher.Infrastructure.Hashing;
using PuxDesignFileWatcher.Infrastructure.Storage;

namespace PuxDesignFileWatcher.Infrastructure;

/// <summary>
/// Infrastructure service registration.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers infrastructure implementations.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton(new AnalysisLimitsOptionsAccessor(configuration));
        services.AddSingleton(new StorageOptionsAccessor(configuration));

        services.AddSingleton<IAnalysisLockPort, KeyedAnalysisLockService>();
        services.AddSingleton<IAnalysisLimitsPort, AnalysisLimitsProvider>();
        services.AddSingleton<IManifestPathResolverPort, ManifestPathResolver>();

        services.AddTransient<IFileSystemTraversalPort, FileSystemTraversalService>();
        services.AddTransient<IContentHashPort, Sha256ContentHashProvider>();
        services.AddTransient<IHybridFileStateBuilderPort, HybridFileStateBuilder>();

        services.AddTransient<JsonManifestRepository>();
        services.AddTransient<MessagePackManifestRepository>();
        services.AddTransient<IManifestRepositoryPort, ConfigurableManifestRepository>();

        return services;
    }
}
