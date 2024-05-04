using ArezkiSaba.CodeCleaner.Extensions;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

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
        Console.WriteLine($"[{nameof(CodeCleanerService)}] Workspace initialization...");

        using var workspace = MSBuildWorkspace.Create();
        workspace.LoadMetadataForReferencedProjects = true;
        workspace.WorkspaceFailed += (_, e) =>
        {
            Console.WriteLine($"[{nameof(MSBuildWorkspace)}] {e.Diagnostic.Message}");
        };
        Console.WriteLine($"[{nameof(CodeCleanerService)}] Workspace initialization done.");

        if (_targetLocation.EndsWith(".sln"))
        {
            await HandleSolutionFileAsync(workspace, _targetLocation);
        }
        else
        {
            Console.WriteLine($"[{nameof(CodeCleanerService)}] Workspace scan...");
            var files = Directory.GetDirectories(_targetLocation);
            Console.WriteLine($"[{nameof(CodeCleanerService)}] Workspace scan done.");

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
        Console.WriteLine($"[{nameof(CodeCleanerService)}] Solution opening ({new FileInfo(slnFile).Name})...");
        var solution = await workspace.OpenSolutionAsync(slnFile);
        Console.WriteLine($"[{nameof(CodeCleanerService)}] Solution opening done.");

        Console.WriteLine($"[{nameof(CodeCleanerService)}] Solution formatting...");
        await workspace.CleanAndRefactorAsync();
        Console.WriteLine($"[{nameof(CodeCleanerService)}] Solution formatting done.");
    }

    #endregion
}
