using Microsoft.Extensions.Configuration;

namespace PuxDesignFileWatcher.Infrastructure.Configuration;

/// <summary>
/// Reads and validates storage configuration.
/// </summary>
public sealed class StorageOptionsAccessor
{
    private readonly StorageOptions _options;

    public StorageOptionsAccessor(IConfiguration configuration)
    {
        _options = new StorageOptions();

        var section = configuration.GetSection(StorageOptions.SectionName);

        if (section["StorageFormat"] is { Length: > 0 } nestedFormat)
        {
            _options.StorageFormat = nestedFormat;
        }

        if (section["BasePath"] is { Length: > 0 } nestedBasePath)
        {
            _options.BasePath = nestedBasePath;
        }

        if (section["Mode"] is { Length: > 0 } nestedMode)
        {
            _options.Mode = nestedMode;
        }

        if (configuration["StorageFormat"] is { Length: > 0 } format)
        {
            _options.StorageFormat = format;
        }

        if (configuration["BasePath"] is { Length: > 0 } basePath)
        {
            _options.BasePath = basePath;
        }

        if (configuration["Mode"] is { Length: > 0 } mode)
        {
            _options.Mode = mode;
        }
    }

    public StorageFormat StorageFormat => Enum.TryParse<StorageFormat>(_options.StorageFormat, true, out var format)
        ? format
        : StorageFormat.Json;

    public ManifestLocationMode Mode => Enum.TryParse<ManifestLocationMode>(_options.Mode, true, out var mode)
        ? mode
        : ManifestLocationMode.AppData;

    public string BasePath => _options.BasePath;
}
