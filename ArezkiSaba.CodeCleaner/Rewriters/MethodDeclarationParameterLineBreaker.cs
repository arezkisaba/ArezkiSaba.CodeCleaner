using ArezkiSaba.CodeCleaner.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Rewriters;

public sealed class MethodDeclarationParameterLineBreaker : CSharpSyntaxRewriter
{
    public override bool VisitIntoStructuredTrivia
    {
        get { return true; }
    }

    public override SyntaxToken VisitToken(
        SyntaxToken token)
    {
        if (token.Parent?.Parent is not BaseMethodDeclarationSyntax baseMethodDeclarationSyntax ||
            !baseMethodDeclarationSyntax.ParameterList.Parameters.Any() ||
            ////invocationExpression.GetInvocationExpressionLength() < 70 ||
            token.Parent.Ancestors().OfType<LocalFunctionStatementSyntax>().Any() ||
            token.Parent.Ancestors().OfType<ParenthesizedLambdaExpressionSyntax>().Any())
        {
            return token;
        }

        var isOpeningParentheseForMethodParameters =
            token.IsKind(SyntaxKind.OpenParenToken) &&
            (token.Parent?.IsKind(SyntaxKind.ParameterList) ?? false) &&
            !(token.Parent?.Parent?.IsKind(SyntaxKind.LocalFunctionStatement) ?? false);
        var isCommaSeparatorForMethodParameters =
            token.IsKind(SyntaxKind.CommaToken) &&
            (token.Parent?.IsKind(SyntaxKind.ParameterList) ?? false) &&
            !(token.Parent?.Parent?.IsKind(SyntaxKind.LocalFunctionStatement) ?? false);

        var declaration = token.Parent.Ancestors().FirstOrDefault(
            obj => obj.IsKind(SyntaxKind.ConstructorDeclaration) || obj.IsKind(SyntaxKind.OperatorDeclaration) || obj.IsKind(SyntaxKind.MethodDeclaration)
        );
        if (declaration == null)
        {
            return token;
        }

        var declarationTrivia = declaration.DescendantTrivia().First(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));
        if (isOpeningParentheseForMethodParameters)
        {
            token = token.WithTrailingTrivia(
                SyntaxFactory.TriviaList(
                    SyntaxTriviaHelper.GetEndOfLine(),
                    declarationTrivia,
                    SyntaxTriviaHelper.GetTab()
                )
            );
        }
        else if (isCommaSeparatorForMethodParameters)
        {
            token = token.WithTrailingTrivia(
                SyntaxFactory.TriviaList(
                    SyntaxTriviaHelper.GetEndOfLine(),
                    declarationTrivia,
                    SyntaxTriviaHelper.GetTab()
                )
            );
        }

        return base.VisitToken(token);
    }
}
