using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class ApplyTypeInference : RefactorOperationBase
{
    public override string Name => nameof(ApplyTypeInference);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        document = document.WithSyntaxRoot(
            new ApplyTypeInferenceSyntaxRewriter(
                semanticModel
            ).Visit(root)
        );
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}