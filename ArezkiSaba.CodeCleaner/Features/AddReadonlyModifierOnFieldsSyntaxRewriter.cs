﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using ArezkiSaba.CodeCleaner.Extensions;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class AddReadonlyModifierOnFieldsSyntaxRewriter : CSharpSyntaxRewriter
{
    private readonly Solution _solution;
    private readonly SemanticModel _semanticModel;

    public AddReadonlyModifierOnFieldsSyntaxRewriter(
        Solution solution,
        SemanticModel semanticModel)
    {
        _solution = solution;
        _semanticModel = semanticModel;
    }

    public override SyntaxNode VisitFieldDeclaration(
        FieldDeclarationSyntax node)
    {
        var hasPrivateKeyWordToken = node.DescendantTokens().Any(obj => obj.IsKind(SyntaxKind.PrivateKeyword));
        var hasReadonlyKeywordToken = node.DescendantTokens().Any(obj => obj.IsKind(SyntaxKind.ReadOnlyKeyword));
        var hasConstKeywordToken = node.DescendantTokens().Any(obj => obj.IsKind(SyntaxKind.ConstKeyword));
        var hasStaticKeywordToken = node.DescendantTokens().Any(obj => obj.IsKind(SyntaxKind.StaticKeyword));
        if (!hasPrivateKeyWordToken || hasReadonlyKeywordToken || hasConstKeywordToken || hasStaticKeywordToken)
        {
            return node;
        }

        var variableDeclarator = node.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
        var symbol = _semanticModel.GetDeclaredSymbol(variableDeclarator);
        var reference = SymbolFinder.FindReferencesAsync(symbol, _solution)
            .GetAwaiter()
            .GetResult()
            .FirstOrDefault();
        var referenceLocations = reference.Locations.ToList();
        var canAddReadonlyModifier = CanAddReadonlyModifier(referenceLocations);
        if (canAddReadonlyModifier)
        {
            node = node.AddModifiers(
                SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword)
                    .WithTrailingTrivia(SyntaxTriviaHelper.GetWhitespace())
            );
        }

        return base.VisitFieldDeclaration(node);
    }

    #region Private use

    private bool CanAddReadonlyModifier(
        List<ReferenceLocation> referenceLocations)
    {
        if (!referenceLocations.Any())
        {
            return true;
        }

        var canAddReadonlyModifier = true;
        foreach (var referenceLocation in referenceLocations)
        {
            var referencedNode = referenceLocation.Document.GetSyntaxRootAsync()
                .GetAwaiter()
                .GetResult()
                .FindNode(referenceLocation.Location.SourceSpan);
            var assignmentExpression = referencedNode.Ancestors()
                .OfType<AssignmentExpressionSyntax>()
                .FirstOrDefault();
            var methodDeclaration = referencedNode.Ancestors()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault();
            var accessorDeclaration = referencedNode.Ancestors()
                .OfType<AccessorDeclarationSyntax>()
                .FirstOrDefault(obj => obj.IsKind(SyntaxKind.SetAccessorDeclaration));
            var isAssignedFromMethod = assignmentExpression != null && methodDeclaration != null;
            var isReferencedFromPropertySetter = accessorDeclaration != null;

            if (isAssignedFromMethod || isReferencedFromPropertySetter)
            {
                canAddReadonlyModifier = false;
                break;
            }
        }

        return canAddReadonlyModifier;
    }

    #endregion
}
