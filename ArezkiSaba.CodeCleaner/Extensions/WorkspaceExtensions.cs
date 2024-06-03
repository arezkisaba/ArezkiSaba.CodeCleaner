﻿using Microsoft.CodeAnalysis;
using ArezkiSaba.CodeCleaner.Features;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class WorkspaceExtensions
{
    public static async Task<Workspace> RefactorAsync(
        this Workspace workspace,
        bool smallRefactor = true,
        bool displayOutput = true)
    {
        var funcs = new List<(RefactorOperationBase, Func<Project, Task<bool>> predicate)>();
        if (!smallRefactor)
        {
            funcs.Add((new DeleteRegions(), (project) => Task.FromResult(true)));
        }

        if (!smallRefactor)
        {
            funcs.Add((new ApplyTypeInference(), (project) => Task.FromResult(true)));
        }

        funcs.Add((new AddReadonlyModifierOnFields(), (project) => Task.FromResult(true)));
        funcs.Add((new AddSealedModifierOnClasses(), async (project) => await project.IsNonNugetProjectAsync()));
        funcs.Add((new DeleteDuplicatedEmptyLines(), (project) => Task.FromResult(true)));
        if (!smallRefactor)
        {
            funcs.Add((new SortClassMembers(), (project) => Task.FromResult(true)));
        }

        funcs.Add((new SortFieldsWithPropfullProperties(), (project) => Task.FromResult(true)));
        funcs.Add((new SortUsingDirectives(), (project) => Task.FromResult(true)));
        if (!smallRefactor)
        {
            funcs.Add((new AddPrivateUseRegion(), (project) => Task.FromResult(true)));
        }

        funcs.Add((new DeleteDuplicatedUsingDirectives(), (project) => Task.FromResult(true)));
        funcs.Add((new DeleteEmptyLinesAroundBraces(), (project) => Task.FromResult(true)));
        funcs.Add((new RenameFields(), (project) => Task.FromResult(true)));
        funcs.Add((new RenameEventFields(), (project) => Task.FromResult(true)));
        funcs.Add((new RenameProperties(), (project) => Task.FromResult(true)));
        funcs.Add((new RenameMethods(), (project) => Task.FromResult(true)));
        funcs.Add((new RenameLocalVariables(), (project) => Task.FromResult(true)));
        funcs.Add((new RenameParameters(), (project) => Task.FromResult(true)));
        funcs.Add((new RenameUnusedMethodParameters(), (project) => Task.FromResult(true)));
        funcs.Add((new RenameAsyncMethod(), (project) => Task.FromResult(true)));
        funcs.Add((new FormatEqualsValueClause(), (project) => Task.FromResult(true)));

        if (!smallRefactor)
        {
            funcs.Add((new FormatAssignmentExpression(), (project) => Task.FromResult(true)));
            funcs.Add((new FormatInitializerExpression(), (project) => Task.FromResult(true)));
            funcs.Add((new FormatAnonymousObjectCreationExpression(), (project) => Task.FromResult(true)));
            funcs.Add((new FormatArgumentList(), (project) => Task.FromResult(true)));
            funcs.Add((new FormatLambdaExpression(), (project) => Task.FromResult(true)));
            funcs.Add((new FormatMemberAccessExpression(), (project) => Task.FromResult(true)));
            funcs.Add((new FormatCode(), (project) => Task.FromResult(true)));
        }

        var i = 0;
        var operationCount = workspace.CurrentSolution.Projects.SelectMany(p => p.Documents.Where(d => !d.IsAutoGenerated())).Count() * funcs.Count;
        var percent = 0d;

        var currentSolution = workspace.CurrentSolution;
        foreach (var projectId in workspace.CurrentSolution.ProjectIds)
        {
            var currentProject = currentSolution.GetProject(projectId);
            foreach (var documentId in currentProject.DocumentIds)
            {
                var currentDocument = currentProject.GetDocument(documentId);
                var isAutoGenerated = currentDocument.IsAutoGenerated();
                if (isAutoGenerated)
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

                    if (displayOutput)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        ClearCurrentConsoleLine();
                        Console.WriteLine($"[{percent.ToString("F2")} %] Executing module {refactorOperation.Name} on {currentDocument.Name}...");
                    }

                    var result = await refactorOperation.StartAsync(currentDocument, currentSolution);
                    currentDocument = result.Document;
                    currentProject = result.Project;
                    currentSolution = result.Solution;
                    i++;
                    percent = (double)i / operationCount * 100;

                    if (displayOutput)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop - 1);
                        ClearCurrentConsoleLine();
                        Console.WriteLine($"[{percent.ToString("F2")} %] Module {refactorOperation.Name} executed on {currentDocument.Name}");
                    }
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
        var currentLineCursor = Console.CursorTop;
        Console.SetCursorPosition(0, Console.CursorTop);
        Console.Write(new string(' ', Console.WindowWidth));
        Console.SetCursorPosition(0, currentLineCursor);
    }

    #endregion
}
