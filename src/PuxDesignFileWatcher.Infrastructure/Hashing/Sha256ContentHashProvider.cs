using System.Security.Cryptography;
using PuxDesignFileWatcher.Application.Ports;

namespace PuxDesignFileWatcher.Infrastructure.Hashing;

/// <summary>
/// Computes SHA-256 hash from file content.
/// </summary>
public sealed class Sha256ContentHashProvider : IContentHashPort
{
    /// <inheritdoc />
    public async Task<string> ComputeHashAsync(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 1024 * 16,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        var hash = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(hash);
    }
}
