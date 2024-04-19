using ArezkiSaba.CodeCleaner.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Rewriters;

public sealed class InvocationExpressionArgumentLineBreaker : CSharpSyntaxRewriter
{
    public override bool VisitIntoStructuredTrivia
    {
        get { return true; }
    }

    public override SyntaxToken VisitToken(
        SyntaxToken token)
    {
        if (token.Parent is not ArgumentListSyntax argumentList ||
            token.Parent.Ancestors().OfType<LocalFunctionStatementSyntax>().Any() ||
            token.Parent.Ancestors().OfType<ParenthesizedLambdaExpressionSyntax>().Any() ||
            argumentList.Arguments.Count < 3)
        {
            return token;
        }

        var isOpeningParentheseForMethodParameters =
            token.IsKind(SyntaxKind.OpenParenToken) &&
            (token.Parent?.IsKind(SyntaxKind.ArgumentList) ?? false) &&
            !(token.Parent?.Parent?.IsKind(SyntaxKind.LocalFunctionStatement) ?? false);
        var isCommaSeparatorForMethodParameters =
            token.IsKind(SyntaxKind.CommaToken) &&
            (token.Parent?.IsKind(SyntaxKind.ArgumentList) ?? false) &&
            !(token.Parent?.Parent?.IsKind(SyntaxKind.LocalFunctionStatement) ?? false);
        var isClosingParentheseForMethodParameters =
            token.IsKind(SyntaxKind.CloseParenToken) &&
            (token.Parent?.IsKind(SyntaxKind.ArgumentList) ?? false) &&
            !(token.Parent?.Parent?.IsKind(SyntaxKind.LocalFunctionStatement) ?? false);

        var statement = token.Parent.Ancestors().FirstOrDefault(
            obj => obj.IsKind(SyntaxKind.ExpressionStatement) || obj.IsKind(SyntaxKind.ReturnStatement)
        );
        if (statement == null)
        {
            return token;
        }

        var declarationTrivia = statement.DescendantTrivia().First(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));
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
        else if (isClosingParentheseForMethodParameters &&
            !token.LeadingTrivia.Any(obj => obj.IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            token = token.WithLeadingTrivia(
                SyntaxFactory.TriviaList(
                    SyntaxTriviaHelper.GetEndOfLine(),
                    declarationTrivia
                )
            );
        }

        return base.VisitToken(token);
    }
}
