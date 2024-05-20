using ArezkiSaba.CodeCleaner.Features.Bases;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class DuplicatedEmptyLinesRemover
{
    public async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        document = document.WithSyntaxRoot(
            new DuplicatedEmptyLinesRemoverSyntaxRewriter(
            ).Visit(root)
        );
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}