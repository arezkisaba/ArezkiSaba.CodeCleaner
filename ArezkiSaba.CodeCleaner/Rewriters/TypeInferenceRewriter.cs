using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Rewriters;

public sealed class TypeInferenceRewriter : CSharpSyntaxRewriter
{
    private readonly SemanticModel _semanticModel;

    public TypeInferenceRewriter(
        SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    public override SyntaxNode VisitLocalDeclarationStatement(
        LocalDeclarationStatementSyntax node)
    {
        if (node.Declaration.Variables.Count > 1 ||
            node.Declaration.Variables[0].Initializer == null ||
            node.Modifiers.Any(obj => obj.IsKind(SyntaxKind.ConstKeyword)))
        {
            return node;
        }
        
        var declarator = node.Declaration.Variables.First();
        var variableTypeName = node.Declaration.Type;

        var variableType = (ITypeSymbol)_semanticModel
            .GetSymbolInfo(variableTypeName)
            .Symbol;
        var initializerInfo = _semanticModel.GetTypeInfo(declarator.Initializer.Value);
        if (variableType == initializerInfo.Type)
        {
            var varTypeName = SyntaxFactory.IdentifierName("var")
                .WithLeadingTrivia(variableTypeName.GetLeadingTrivia())
                .WithTrailingTrivia(variableTypeName.GetTrailingTrivia());
            return node.ReplaceNode(variableTypeName, varTypeName);
        }
        else
        {
            return node;
        }
    }
}
