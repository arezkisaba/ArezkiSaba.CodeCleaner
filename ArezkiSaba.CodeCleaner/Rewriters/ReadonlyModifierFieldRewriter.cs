using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace ArezkiSaba.CodeCleaner.Rewriters;

public sealed class ReadonlyModifierFieldRewriter : CSharpSyntaxRewriter
{
    private readonly Solution _solution;
    private readonly SyntaxNode _root;
    private readonly SemanticModel _semanticModel;

    public ReadonlyModifierFieldRewriter(
        Solution solution,
        SyntaxNode root,
        SemanticModel semanticModel)
    {
        _solution = solution;
        _root = root;
        _semanticModel = semanticModel;
    }

    public override SyntaxNode VisitFieldDeclaration(
        FieldDeclarationSyntax node)
    {
        var variableDeclarator = node.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
        var variableDeclaratorSymbol = _semanticModel.GetDeclaredSymbol(variableDeclarator);
        var hasPrivateKeyWordToken = node.DescendantTokens().Any(obj => obj.IsKind(SyntaxKind.PrivateKeyword));
        var hasReadonlyKeywordToken = node.DescendantTokens().Any(obj => obj.IsKind(SyntaxKind.ReadOnlyKeyword));

        if (!hasPrivateKeyWordToken || hasReadonlyKeywordToken)
        {
            return node;
        }

        var canAddReadonlyModifier = true;
        var variableDeclaratorReference = SymbolFinder.FindReferencesAsync(variableDeclaratorSymbol, _solution).GetAwaiter().GetResult().FirstOrDefault();
        var referenceLocations = variableDeclaratorReference.Locations.ToList();
        if (referenceLocations.Any())
        {
            foreach (var referenceLocation in referenceLocations)
            {
                var foundNode = _root.FindNode(referenceLocation.Location.SourceSpan);
                var simpleAssignmentExpression = foundNode.Ancestors().FirstOrDefault(obj => obj.IsKind(SyntaxKind.SimpleAssignmentExpression));
                var constructorDeclaration = foundNode.Ancestors().FirstOrDefault(obj => obj.IsKind(SyntaxKind.ConstructorDeclaration));

                if (simpleAssignmentExpression != null && constructorDeclaration == null)
                {
                    canAddReadonlyModifier = false;
                    break;
                }
            }
        }

        if (canAddReadonlyModifier)
        {
            node = node.AddModifiers(SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword).WithTrailingTrivia());
        }

        return base.VisitFieldDeclaration(node);
    }
}
