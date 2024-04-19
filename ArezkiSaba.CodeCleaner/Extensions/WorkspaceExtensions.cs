﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;
using System.Reflection;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class WorkspaceExtensions
{
    public static async Task<Workspace> CleanAndRefactorAsync(
        this Workspace workspace)
    {
        await workspace.CleanAsync();
        await workspace.RefactorAsync();
        return workspace;
    }

    #region Private use

    private static async Task CleanAsync(
        this Workspace workspace)
    {
        var cleaningFuncs = new List<Func<Document, Task<Document>>>();
        cleaningFuncs.Add((document) => document.StartTypeInferenceRewriterAsync());
        cleaningFuncs.Add((document) => document.StartReadonlyModifierFieldRewriterAsync());
        cleaningFuncs.Add((document) => document.StartUsingDirectiveSorterAsync());
        cleaningFuncs.Add((document) => document.StartDuplicatedUsingDirectiveRemoverAsync());
        cleaningFuncs.Add((document) => document.StartEmptyLinesBracesRemoverAsync());
        cleaningFuncs.Add((document) => document.StartDuplicatedEmptyLinesRemoverAsync());
        cleaningFuncs.Add((document) => document.ReorderClassMembersAsync());
        cleaningFuncs.Add((document) => document.StartRegionInserterAsync());
        cleaningFuncs.Add((document) => document.StartMethodDeclarationParameterLineBreakerAsync());
        cleaningFuncs.Add((document) => document.StartInvocationExpressionArgumentLineBreakerAsync());

        var newSolution = workspace.CurrentSolution;
        var projectIds = workspace.CurrentSolution.ProjectIds;

        foreach (var projectId in projectIds)
        {
            var project = newSolution.GetProject(projectId);
            var documentIds = project.DocumentIds;
            foreach (var documentId in documentIds)
            {
                var originalDocument = project.GetDocument(documentId);
                var originalRoot = await originalDocument.GetSyntaxRootAsync();
                if (originalDocument.IsAutoGenerated() || originalRoot.BeginsWithAutoGeneratedComment())
                {
                    continue;
                }

                var updatedDocument = originalDocument;
                foreach (var cleaningFunc in cleaningFuncs)
                {
                    updatedDocument = await cleaningFunc(updatedDocument);
                    updatedDocument = await Formatter.FormatAsync(updatedDocument);
                }
                
                project = updatedDocument.Project;
                newSolution = project.Solution;
            }
        }

        ApplyChanges(workspace, newSolution);
    }

    private static async Task RefactorAsync(
        this Workspace workspace)
    {
        var refactoringFuncs = new List<Func<Document, Solution, Task<Solution>>>();
        refactoringFuncs.Add((document, solution) => document.StartFieldRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartEventFieldRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartMethodRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartLocalVariableRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartUnusedMethodParameterRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartAsyncMethodRenamerAsync(solution));

        foreach (var refactoringFunc in refactoringFuncs)
        {
            var newSolution = workspace.CurrentSolution;
            var projectIds = workspace.CurrentSolution.ProjectIds;

            foreach (var projectId in projectIds)
            {
                var project = workspace.CurrentSolution.GetProject(projectId);
                var documentIds = project.DocumentIds;
                foreach (var documentId in documentIds)
                {
                    var originalDocument = project.GetDocument(documentId);
                    var originalRoot = await originalDocument.GetSyntaxRootAsync();
                    if (originalDocument.IsAutoGenerated() || originalRoot.BeginsWithAutoGeneratedComment())
                    {
                        continue;
                    }

                    newSolution = await refactoringFunc(originalDocument, newSolution);
                }
            }

            ApplyChanges(workspace, newSolution);
        }
    }

    private static void ApplyChanges(
        this Workspace workspace,
        Solution newSolution)
    {
        string sender;
        if (string.IsNullOrWhiteSpace(newSolution.FilePath))
        {
            sender = "InMemory solution";
        }
        else
        {
            sender = new FileInfo(newSolution.FilePath).Name;
        }

        if (!ReferenceEquals(newSolution, workspace.CurrentSolution) &&
            workspace.TryApplyChanges(newSolution))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{sender} => Changes applied");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{sender} => No changes applied");
        }

        Console.ResetColor();
    }

    #endregion
}
