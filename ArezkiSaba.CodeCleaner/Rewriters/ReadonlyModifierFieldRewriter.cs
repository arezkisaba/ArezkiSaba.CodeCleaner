using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace ArezkiSaba.CodeCleaner.Rewriters;

public sealed class ReadonlyModifierFieldRewriter : CSharpSyntaxRewriter
{
    private readonly Solution _solution;
    private readonly SemanticModel _semanticModel;

    public ReadonlyModifierFieldRewriter(
        Solution solution,
        SemanticModel semanticModel)
    {
        _solution = solution;
        _semanticModel = semanticModel;
    }

    public override SyntaxNode VisitFieldDeclaration(
        FieldDeclarationSyntax node)
    {
        var variableDeclarator = node.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
        var variableDeclaratorSymbol = _semanticModel.GetDeclaredSymbol(variableDeclarator);
        var hasPrivateKeyWordToken = node.DescendantTokens().Any(obj => obj.IsKind(SyntaxKind.PrivateKeyword));
        var hasReadonlyKeywordToken = node.DescendantTokens().Any(obj => obj.IsKind(SyntaxKind.ReadOnlyKeyword));
        var hasStaticKeywordToken = node.DescendantTokens().Any(obj => obj.IsKind(SyntaxKind.StaticKeyword));

        if (!hasPrivateKeyWordToken || hasReadonlyKeywordToken || hasStaticKeywordToken)
        {
            return node;
        }

        var variableDeclaratorReference = SymbolFinder.FindReferencesAsync(variableDeclaratorSymbol, _solution)
            .GetAwaiter()
            .GetResult()
            .FirstOrDefault();
        var referenceLocations = variableDeclaratorReference.Locations.ToList();
        if (!referenceLocations.Any())
        {
            return node;
        }

        var canAddReadonlyModifier = CanAddReadonlyModifier(referenceLocations);
        if (canAddReadonlyModifier)
        {
            node = node.AddModifiers(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword).WithTrailingTrivia());
        }

        return base.VisitFieldDeclaration(node);
    }

    #region Private use

    private bool CanAddReadonlyModifier(
        List<ReferenceLocation> referenceLocations)
    {
        var canAddReadonlyModifier = true;
        foreach (var referenceLocation in referenceLocations)
        {
            var referencedNode = referenceLocation.Document.GetSyntaxRootAsync()
                .GetAwaiter()
                .GetResult()
                .FindNode(referenceLocation.Location.SourceSpan);
            var simpleAssignmentExpression = referencedNode.Ancestors()
                .FirstOrDefault(obj => obj.IsKind(SyntaxKind.SimpleAssignmentExpression));
            var constructorDeclaration = referencedNode.Ancestors()
                .FirstOrDefault(obj => obj.IsKind(SyntaxKind.ConstructorDeclaration));

            if (simpleAssignmentExpression != null && constructorDeclaration == null)
            {
                canAddReadonlyModifier = false;
                break;
            }
        }

        return canAddReadonlyModifier;
    }

    #endregion
}
