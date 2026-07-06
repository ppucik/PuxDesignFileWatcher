using PuxDesignFileWatcher.Infrastructure.Concurrency;

namespace PuxDesignFileWatcher.IntegrationTests.Concurrency;

public class KeyedAnalysisLockServiceTests
{
    [Fact]
    public async Task AcquireAsync_ForSamePath_ShouldSerializeAccess()
    {
        var service = new KeyedAnalysisLockService();
        var rootPath = Path.Combine(Path.GetTempPath(), "PuxDesignFileWatcherLock", Guid.NewGuid().ToString("N"));

        var firstLock = await service.AcquireAsync(rootPath, CancellationToken.None);

        var secondStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var secondTask = Task.Run(async () =>
        {
            secondStarted.SetResult(true);
            await using var secondLock = await service.AcquireAsync(rootPath, CancellationToken.None);
        });

        await secondStarted.Task;
        await Task.Delay(150);

        Assert.False(secondTask.IsCompleted);

        await firstLock.DisposeAsync();
        await secondTask;
    }
}
