using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class DeleteDuplicatedEmptyLines : RefactorOperationBase
{
    public override string Name => nameof(DeleteDuplicatedEmptyLines);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        document = document.WithSyntaxRoot(
            new DeleteDuplicatedEmptyLinesSyntaxRewriter(
            ).Visit(root)
        );
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}