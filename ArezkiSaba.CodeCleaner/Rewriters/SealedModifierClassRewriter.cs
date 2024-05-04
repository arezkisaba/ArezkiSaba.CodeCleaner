using ArezkiSaba.CodeCleaner.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Rewriters;

public sealed class SealedModifierClassRewriter : CSharpSyntaxRewriter
{
    private readonly Solution _solution;
    private readonly SemanticModel _semanticModel;
    private readonly IList<BaseTypeSyntax> _allBaseTypes = [];

    public SealedModifierClassRewriter(
        Solution solution,
        SemanticModel semanticModel,
        IList<TypeDeclarationSyntax> allTypeDeclarations)
    {
        _solution = solution;
        _semanticModel = semanticModel;
        _allBaseTypes = allTypeDeclarations.SelectMany(t => t.BaseList?.Types ?? []).ToList();
    }

    public override SyntaxNode VisitClassDeclaration(
        ClassDeclarationSyntax node)
    {
        var canAddSealedModifier = CanAddSealedModifier(node);
        if (canAddSealedModifier)
        {
            node = node.AddModifiers(SyntaxFactory.Token(SyntaxKind.SealedKeyword).WithTrailingTrivia());
        }

        return base.VisitClassDeclaration(node);
    }

    #region Private use

    private bool CanAddSealedModifier(
        ClassDeclarationSyntax node)
    {
        var hasAlreadySealedModifier = node.Modifiers.Any(obj => obj.IsKind(SyntaxKind.SealedKeyword));
        var hasStaticModifier = node.Modifiers.Any(obj => obj.IsKind(SyntaxKind.StaticKeyword));
        if (hasAlreadySealedModifier ||
            hasStaticModifier)
        {
            return false;
        }

        var symbol = _semanticModel.GetDeclaredSymbol(node);
        var areAllBaseTypeDifferentThanNodeType = _allBaseTypes.All(baseType =>
        {
            return baseType.Type.GetName() != symbol.Name;
        });
        return areAllBaseTypeDifferentThanNodeType;
    }

    #endregion
}
