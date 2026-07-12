using Microsoft.AspNetCore.Mvc;
using PuxDesignFileWatcher.Application.Abstractions.CQRS;
using PuxDesignFileWatcher.Application.UseCases.AnalyzeDirectory;
using PuxDesignFileWatcher.Domain.Changes;
using PuxDesignFileWatcher.Web.Models;
using System.Diagnostics;

namespace PuxDesignFileWatcher.Web.Controllers;

public class HomeController : Controller
{
    private readonly ICommandHandler<AnalyzeDirectoryCommand, DirectoryAnalysisResult> _analyzeDirectory;

    public HomeController(ICommandHandler<AnalyzeDirectoryCommand, DirectoryAnalysisResult> analyzeDirectory)
    {
        _analyzeDirectory = analyzeDirectory;
    }

    public IActionResult Index()
    {
        return View(new AnalyzeDirectoryPageViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(AnalyzeDirectoryPageViewModel pageModel, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(pageModel);
        }

        if (!Directory.Exists(pageModel.Input.RootPath))
        {
            ModelState.AddModelError(nameof(pageModel.Input.RootPath), "The provided directory path does not exist.");
            return View(pageModel);
        }

        DirectoryAnalysisResult result;

        try
        {
            result = await _analyzeDirectory.HandleAsync(
                new AnalyzeDirectoryCommand(pageModel.Input.RootPath),
                cancellationToken);
        }
        catch (DirectoryNotFoundException)
        {
            ModelState.AddModelError(nameof(pageModel.Input.RootPath), "The provided directory path does not exist.");
            return View(pageModel);
        }
        catch (DirectoryAnalysisValidationException ex)
        {
            ModelState.AddModelError(nameof(pageModel.Input.RootPath), ex.Message);
            return View(pageModel);
        }

        pageModel.NewFiles = MapByType(result, ChangeType.New);
        pageModel.ChangedFiles = MapByType(result, ChangeType.Changed);
        pageModel.DeletedFiles = MapByType(result, ChangeType.Deleted);
        pageModel.Message = $"Analysis completed at {result.AnalyzedAtUtc:O}.";

        return View(pageModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

    private static IReadOnlyCollection<AnalysisResultItemViewModel> MapByType(
        DirectoryAnalysisResult result,
        ChangeType type)
        => result.Changes
            .Where(change => change.ChangeType == type)
            .Select(change => new AnalysisResultItemViewModel(change.RelativePath, change.Version))
            .OrderBy(change => change.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
