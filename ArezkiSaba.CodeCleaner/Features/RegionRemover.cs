﻿using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class RegionRemover
{
    public async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        document = document.WithSyntaxRoot(
            new RegionRemoverSyntaxRewriter(
            ).Visit(root)
        );
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}