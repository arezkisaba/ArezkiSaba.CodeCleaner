using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class TransformAnonymousToImplicitObjectCreationSyntaxRewriter : CSharpSyntaxRewriter
{
    private readonly Solution _solution;
    private readonly SemanticModel _semanticModel;

    public TransformAnonymousToImplicitObjectCreationSyntaxRewriter(
        Solution solution,
        SemanticModel semanticModel)
    {
        _solution = solution;
        _semanticModel = semanticModel;
    }

    public override SyntaxNode VisitAnonymousObjectCreationExpression(
        AnonymousObjectCreationExpressionSyntax node)
    {
        return SyntaxFactory.ImplicitObjectCreationExpression()
            .WithArgumentList(SyntaxFactory.ArgumentList())
            .WithInitializer(SyntaxFactory.InitializerExpression(
                SyntaxKind.ObjectInitializerExpression,
                SyntaxFactory.SeparatedList<ExpressionSyntax>(node.Initializers.Select(init =>
                    SyntaxFactory.AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        (init.NameEquals != null ? (ExpressionSyntax)SyntaxFactory.IdentifierName(init.NameEquals.Name.Identifier) : (ExpressionSyntax)init.Expression),
                init.Expression)))))
            .WithLeadingTrivia(node.GetLeadingTrivia())
            .WithTrailingTrivia(node.GetTrailingTrivia());
    }
}
