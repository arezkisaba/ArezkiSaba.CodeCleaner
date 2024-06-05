using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;
using ArezkiSaba.CodeCleaner.Extensions;

namespace ArezkiSaba.CodeCleaner;

public sealed class CodeCleanerService
{
    private readonly string _targetLocation;

    public CodeCleanerService(
        string targetLocation)
    {
        _targetLocation = targetLocation;

        MSBuildLocator.RegisterDefaults();
    }

    public async Task StartAsync()
    {
        LogHelper.Log($"[{nameof(CodeCleanerService)}] Workspace initialization...");

        using var workspace = MSBuildWorkspace.Create();
        workspace.LoadMetadataForReferencedProjects = true;
        workspace.WorkspaceFailed += (_, e) =>
        {
            if (e.Diagnostic.Message.Contains("Unable to load the service index for source"))
            {
                return;
            }

            Console.WriteLine($"[{nameof(MSBuildWorkspace)}] {e.Diagnostic.Message}");
        };
        LogHelper.Log($"[{nameof(CodeCleanerService)}] Workspace initialization done.");

        if (_targetLocation.EndsWith(".sln"))
        {
            await HandleSolutionFileAsync(workspace, _targetLocation);
        }
        else
        {
            LogHelper.Log($"[{nameof(CodeCleanerService)}] Workspace scan...");
            var files = Directory.GetDirectories(_targetLocation);
            LogHelper.Log($"[{nameof(CodeCleanerService)}] Workspace scan done.");

            foreach (var file in files)
            {
                var slnFiles = Directory.GetFiles(file, "*.sln");
                foreach (var slnFile in slnFiles)
                {
                    await HandleSolutionFileAsync(workspace, slnFile);
                }
            }
        }
    }

    #region Private use

    private static async Task HandleSolutionFileAsync(
        MSBuildWorkspace workspace,
        string slnFile)
    {
        LogHelper.Log($"[{nameof(CodeCleanerService)}] Solution opening ({new FileInfo(slnFile).Name})...");
        var solution = await workspace.OpenSolutionAsync(slnFile);
        LogHelper.Log($"[{nameof(CodeCleanerService)}] Solution opening done.");

        LogHelper.Log($"[{nameof(CodeCleanerService)}] Solution formatting...");
        await workspace.RefactorAsync();
        LogHelper.Log($"[{nameof(CodeCleanerService)}] Solution formatting done.");
    }

    #endregion
}
