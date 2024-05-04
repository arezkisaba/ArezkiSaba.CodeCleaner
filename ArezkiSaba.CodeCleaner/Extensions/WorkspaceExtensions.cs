﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

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
        var cleaningFuncs = new List<(Func<Document, Task<Document>> func, Func<Project, Task<bool>> predicate)>();
        cleaningFuncs.Add(((document) => document.StartTypeInferenceRewriterAsync(), (project) => Task.FromResult(true)));
        cleaningFuncs.Add(((document) => document.StartReadonlyModifierFieldRewriterAsync(), (project) => Task.FromResult(true)));
        cleaningFuncs.Add(((document) => document.StartSealedModifierClassRewriterAsync(), async (project) => await project.IsNonNugetProjectAsync()));
        cleaningFuncs.Add(((document) => document.StartUsingDirectiveSorterAsync(), (project) => Task.FromResult(true)));
        cleaningFuncs.Add(((document) => document.StartDuplicatedUsingDirectiveRemoverAsync(), (project) => Task.FromResult(true)));
        cleaningFuncs.Add(((document) => document.ReorderClassMembersAsync(), (project) => Task.FromResult(true)));
        cleaningFuncs.Add(((document) => document.StartEmptyLinesBracesRemoverAsync(), (project) => Task.FromResult(true)));
        cleaningFuncs.Add(((document) => document.StartDuplicatedEmptyLinesRemoverAsync(), (project) => Task.FromResult(true)));
        cleaningFuncs.Add(((document) => document.StartRegionInserterAsync(), (project) => Task.FromResult(true)));
        cleaningFuncs.Add(((document) => document.StartMethodDeclarationParameterLineBreakerAsync(), (project) => Task.FromResult(true)));
        cleaningFuncs.Add(((document) => document.StartInvocationExpressionArgumentLineBreakerAsync(), (project) => Task.FromResult(true)));

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
                foreach (var (func, predicate) in cleaningFuncs)
                {
                    var canExecuteFunc = await predicate(project);
                    if (!canExecuteFunc)
                    {
                        continue;
                    }

                    updatedDocument = await func(updatedDocument);
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
        var refactoringFuncs = new List<Func<Document, Solution, Task<(Document document, Solution solution)>>>();
        refactoringFuncs.Add((document, solution) => document.StartFieldRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartEventFieldRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartPropertyRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartMethodRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartLocalVariableRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartParameterRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartUnusedMethodParameterRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.StartAsyncMethodRenamerAsync(solution));
        refactoringFuncs.Add((document, solution) => document.ReorderFieldsWithPropertiesWhenPossibleAsync(solution));

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
                    var updatedDocument = originalDocument;
                    var originalRoot = await originalDocument.GetSyntaxRootAsync();
                    if (originalDocument.IsAutoGenerated() || originalRoot.BeginsWithAutoGeneratedComment())
                    {
                        continue;
                    }

                    var (document, solution) = await refactoringFunc(updatedDocument, newSolution);
                    newSolution = solution;
                    updatedDocument = await Formatter.FormatAsync(document);
                }
            }

            ApplyChanges(workspace, newSolution);
        }
    }

    private static void ApplyChanges(
        this Workspace workspace,
        Solution newSolution)
    {
        if (!ReferenceEquals(newSolution, workspace.CurrentSolution))
        {
            workspace.TryApplyChanges(newSolution);
        }
    }

    #endregion
}
