using ArezkiSaba.CodeCleaner.Extensions;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis.MSBuild;

namespace ArezkiSaba.CodeCleaner;

public sealed class CodeCleanerService
{
    private readonly string _sourceCodeLocation;

    public CodeCleanerService(
        string sourceCodeLocation)
    {
        _sourceCodeLocation = sourceCodeLocation;

        Console.ForegroundColor = ConsoleColor.White;
        MSBuildLocator.RegisterDefaults();
    }

    public async Task StartAsync()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"[{nameof(CodeCleanerService)}] Workspace initialization...");

        using var workspace = MSBuildWorkspace.Create();
        workspace.LoadMetadataForReferencedProjects = true;
        workspace.WorkspaceFailed += (_, e) =>
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{nameof(MSBuildWorkspace)}] {e.Diagnostic.Message}");
            Console.ResetColor();
        };
        Console.WriteLine($"[{nameof(CodeCleanerService)}] Workspace initialization done.");

        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"[{nameof(CodeCleanerService)}] Solution scan...");
        var files = Directory.GetDirectories(_sourceCodeLocation);
        Console.WriteLine($"[{nameof(CodeCleanerService)}] Solution scan done.");

        foreach (var file in files)
        {
            var slnFiles = Directory.GetFiles(file, "*.sln");
            foreach (var slnFile in slnFiles)
            {
                Console.WriteLine($"[{nameof(CodeCleanerService)}] Solution opening ({new FileInfo(slnFile).Name})...");
                var solution = await workspace.OpenSolutionAsync(slnFile);
                Console.WriteLine($"[{nameof(CodeCleanerService)}] Solution opening done.");

                Console.WriteLine($"[{nameof(CodeCleanerService)}] Solution formatting...");
                await workspace.CleanAsync();
                Console.WriteLine($"[{nameof(CodeCleanerService)}] Solution formatting done.");
            }
        }
    }
}
