using PuxDesignFileWatcher.Application.Models;

namespace PuxDesignFileWatcher.Application.Ports;

/// <summary>
/// Provides analysis guard-rail limits from configuration.
/// </summary>
public interface IAnalysisLimitsPort
{
    /// <summary>
    /// Gets currently configured limits.
    /// </summary>
    AnalysisLimitsSettings GetLimits();
}
