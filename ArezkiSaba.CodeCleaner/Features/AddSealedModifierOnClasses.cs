﻿using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class AddSealedModifierOnClasses : RefactorOperationBase
{
    public override string Name => nameof(AddSealedModifierOnClasses);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var allTypeDeclarations = await document.Project.Solution.GetAllTypeDeclarations();
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        document = document.WithSyntaxRoot(
            new AddSealedModifierOnClassesSyntaxRewriter(
                document.Project.Solution,
                semanticModel,
                allTypeDeclarations
            ).Visit(root)
        );
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}