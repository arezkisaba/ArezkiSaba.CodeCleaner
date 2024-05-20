using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class TypeInferenceWriterSyntaxRewriter : CSharpSyntaxRewriter
{
    private readonly SemanticModel _semanticModel;

    public TypeInferenceWriterSyntaxRewriter(
        SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    public override SyntaxNode VisitLocalDeclarationStatement(
        LocalDeclarationStatementSyntax node)
    {
        if (node.Declaration.Variables.Count > 1 ||
            node.Declaration.Variables[0].Initializer == null ||
            node.Declaration.Variables[0].Initializer.Value is LiteralExpressionSyntax literalExpressionSyntax && literalExpressionSyntax.Token.Value == null ||
            node.Modifiers.Any(obj => obj.IsKind(SyntaxKind.ConstKeyword)))
        {
            return node;
        }

        var variableTypeName = node.Declaration.Type;
        var varTypeName = SyntaxFactory.IdentifierName("var")
            .WithLeadingTrivia(variableTypeName.GetLeadingTrivia())
            .WithTrailingTrivia(variableTypeName.GetTrailingTrivia());
        return node.ReplaceNode(variableTypeName, varTypeName);
    }
}
