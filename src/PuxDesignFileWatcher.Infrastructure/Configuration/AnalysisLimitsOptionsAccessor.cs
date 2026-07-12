using Microsoft.Extensions.Configuration;

namespace PuxDesignFileWatcher.Infrastructure.Configuration;

/// <summary>
/// Reads configured analysis limits.
/// </summary>
public sealed class AnalysisLimitsOptionsAccessor
{
    private readonly AnalysisLimitsOptions _options;

    public AnalysisLimitsOptionsAccessor(IConfiguration configuration)
    {
        _options = new AnalysisLimitsOptions();

        var section = configuration.GetSection(AnalysisLimitsOptions.SECTION_NAME);

        if (int.TryParse(section["MaxFileCount"], out var maxFileCount) && maxFileCount > 0)
        {
            _options.MaxFileCount = maxFileCount;
        }

        if (int.TryParse(section["MaxFileSizeMb"], out var maxFileSizeMb) && maxFileSizeMb > 0)
        {
            _options.MaxFileSizeMb = maxFileSizeMb;
        }
    }

    public int MaxFileCount => _options.MaxFileCount;

    public int MaxFileSizeMb => _options.MaxFileSizeMb;
}
