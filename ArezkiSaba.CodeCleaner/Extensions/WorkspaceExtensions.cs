﻿using ArezkiSaba.CodeCleaner.Features;
using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class WorkspaceExtensions
{
    public static async Task<Workspace> RefactorAsync(
        this Workspace workspace)
    {
        var funcs = new List<(RefactorOperationBase, Func<Project, Task<bool>> predicate)>
        {
            (new DeleteRegions(), (project) => Task.FromResult(true)),
            (new ApplyTypeInference(), (project) => Task.FromResult(true)),
            (new AddReadonlyModifierOnFields(), (project) => Task.FromResult(true)),
            (new AddSealedModifierOnClasses(), async (project) => await project.IsNonNugetProjectAsync()),
            (new DeleteDuplicatedEmptyLines(), (project) => Task.FromResult(true)),
            (new SortClassMembers(), (project) => Task.FromResult(true)),
            (new SortFieldsWithPropfullProperties(), (project) => Task.FromResult(true)),
            (new SortUsingDirectives(), (project) => Task.FromResult(true)),
            (new AddPrivateUseRegion(), (project) => Task.FromResult(true)),
            (new DeleteDuplicatedUsingDirectives(), (project) => Task.FromResult(true)),
            (new DeleteEmptyLinesAroundBraces(), (project) => Task.FromResult(true)),
            (new RenameFields(), (project) => Task.FromResult(true)),
            (new RenameEventFields(), (project) => Task.FromResult(true)),
            (new RenameProperties(), (project) => Task.FromResult(true)),
            (new RenameMethods(), (project) => Task.FromResult(true)),
            (new RenameLocalVariables(), (project) => Task.FromResult(true)),
            (new RenameParameters(), (project) => Task.FromResult(true)),
            (new RenameUnusedMethodParameters(), (project) => Task.FromResult(true)),
            (new RenameAsyncMethod(), (project) => Task.FromResult(true)),
            (new FormatCode(), (project) => Task.FromResult(true)),


            ////(document.FormatAsync(solution), (project) => Task.FromResult(true))
        };

        var currentSolution = workspace.CurrentSolution;
        foreach (var projectId in workspace.CurrentSolution.ProjectIds)
        {
            var currentProject = currentSolution.GetProject(projectId);
            foreach (var documentId in currentProject.DocumentIds)
            {
                var currentDocument = currentProject.GetDocument(documentId);
                var rootToUpdate = await currentDocument.GetSyntaxRootAsync();
                if (currentDocument.IsAutoGenerated() || rootToUpdate.BeginsWithAutoGeneratedComment())
                {
                    continue;
                }

                foreach (var (refactorOperation, predicate) in funcs)
                {
                    var canExecuteFunc = await predicate(currentProject);
                    if (!canExecuteFunc)
                    {
                        continue;
                    }

                    ////Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ////ClearCurrentConsoleLine();
                    ////Console.WriteLine($"Executing module {refactorOperation.Name} on {currentDocument.Name}...");

                    var result = await refactorOperation.StartAsync(currentDocument, currentSolution);
                    currentDocument = result.Document;
                    currentProject = result.Project;
                    currentSolution = result.Solution;

                    ////Console.SetCursorPosition(0, Console.CursorTop - 1);
                    ////ClearCurrentConsoleLine();
                    ////Console.WriteLine($"Module {refactorOperation.Name} executed on {currentDocument.Name}");
                }
            }
        }

        ApplyChanges(workspace, currentSolution);
        return workspace;
    }

    #region Private use

    private static void ApplyChanges(
        this Workspace workspace,
        Solution newSolution)
    {
        if (!ReferenceEquals(newSolution, workspace.CurrentSolution))
        {
            var changesApplied = workspace.TryApplyChanges(newSolution);
            if (changesApplied)
            {
            }
            else
            {
            }
        }
    }

    public static void ClearCurrentConsoleLine()
    {
        int currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }

    #endregion
}
