using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Rewriters;

public sealed class TypeInferenceRewriter : CSharpSyntaxRewriter
{
    public TypeInferenceRewriter()
    {
    }

    public override SyntaxNode VisitLocalDeclarationStatement(
        LocalDeclarationStatementSyntax node)
    {
        if (node.Declaration.Variables.Count > 1)
        {
            return node;
        }

        if (node.Declaration.Variables[0].Initializer == null)
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
