﻿using Microsoft.CodeAnalysis;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class AddReadonlyModifierOnFields : RefactorOperationBase
{
    public override string Name => nameof(AddReadonlyModifierOnFields);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        document = document.WithSyntaxRoot(
            new AddReadonlyModifierOnFieldsSyntaxRewriter(
                document.Project.Solution,
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