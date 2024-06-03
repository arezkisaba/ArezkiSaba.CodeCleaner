using Microsoft.CodeAnalysis;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class DeleteRegions : RefactorOperationBase
{
    public override string Name => nameof(DeleteRegions);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        document = document.WithSyntaxRoot(
            new DeleteRegionsSyntaxRewriter(
            ).Visit(root)
        );
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}
