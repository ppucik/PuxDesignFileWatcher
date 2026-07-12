using PuxDesignFileWatcher.Application.Models;
using PuxDesignFileWatcher.Application.Ports;
using PuxDesignFileWatcher.Infrastructure.Configuration;

namespace PuxDesignFileWatcher.Infrastructure.Analysis;

/// <summary>
/// Provides analysis limits from configuration.
/// </summary>
public sealed class AnalysisLimitsProvider : IAnalysisLimitsPort
{
    private readonly AnalysisLimitsOptionsAccessor _limits;

    public AnalysisLimitsProvider(AnalysisLimitsOptionsAccessor limits)
    {
        _limits = limits;
    }

    /// <inheritdoc />
    public AnalysisLimitsSettings GetLimits()
    {
        var bytes = _limits.MaxFileSizeMb * 1024L * 1024L;
        return new AnalysisLimitsSettings(_limits.MaxFileCount, bytes);
    }
}
