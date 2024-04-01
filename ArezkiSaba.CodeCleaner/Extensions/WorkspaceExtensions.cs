﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Formatting;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class WorkspaceExtensions
{
    public static async Task<Workspace> RefactorAsync(
        this Workspace workspace)
    {
        var changesCount = 0;
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

                var updatedDocument = await originalDocument.StartTypeInferenceRewriterAsync();
                updatedDocument = await updatedDocument.StartReadonlyModifierFieldRewriterAsync();
                updatedDocument = await updatedDocument.StartUsingDirectiveSorterAsync();
                updatedDocument = await updatedDocument.StartDuplicatedUsingDirectiveRemoverAsync();
                updatedDocument = await updatedDocument.StartDuplicatedEmptyLinesRemoverAsync();
                updatedDocument = await updatedDocument.StartDuplicatedMethodEmptyLinesRemoverAsync();
                updatedDocument = await updatedDocument.ReorderClassMembersAsync();
                updatedDocument = await updatedDocument.StartUnusedMethodParameterDiscarderAsync(newSolution);
                updatedDocument = await updatedDocument.StartMethodDeclarationParameterLineBreakerAsync();
                updatedDocument = await updatedDocument.StartInvocationExpressionArgumentLineBreakerAsync();
                updatedDocument = await Formatter.FormatAsync(updatedDocument);

                project = updatedDocument.Project;
                newSolution = project.Solution;

                var originalText = (await originalDocument.GetTextAsync()).ToString();
                var updatedText = (await updatedDocument.GetTextAsync()).ToString();
                if (!originalText.Equals(updatedText))
                {
                    changesCount++;
                }
            }
        }

        ApplyChanges(workspace, newSolution, changesCount);
        return workspace;
    }

    #region Private use

    private static void ApplyChanges(
        this Workspace workspace,
        Solution newSolution,
        int changesCount)
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
            Console.WriteLine($"{sender} => Changes applied in {changesCount} files");
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
