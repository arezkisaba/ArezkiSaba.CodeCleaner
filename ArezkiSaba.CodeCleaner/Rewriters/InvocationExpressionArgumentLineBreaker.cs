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

        var indentationTrivia = SyntaxFactory.Whitespace("\t");
        var statementTrivia = statement.DescendantTrivia().First(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));
        if (isOpeningParentheseForMethodParameters &&
            !token.TrailingTrivia.Any(obj => obj.IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            token = token.WithTrailingTrivia(
                SyntaxFactory.TriviaList(
                    SyntaxFactory.EndOfLine(Environment.NewLine),
                    statementTrivia,
                    indentationTrivia
                )
            );
        }
        else if (isCommaSeparatorForMethodParameters &&
            !token.TrailingTrivia.Any(obj => obj.IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            token = token.WithTrailingTrivia(
                SyntaxFactory.TriviaList(
                    SyntaxFactory.EndOfLine(Environment.NewLine),
                    statementTrivia,
                    indentationTrivia
                )
            );
        }
        else if (isClosingParentheseForMethodParameters &&
            !token.LeadingTrivia.Any(obj => obj.IsKind(SyntaxKind.EndOfLineTrivia)))
        {
            token = token.WithLeadingTrivia(
                SyntaxFactory.TriviaList(
                    SyntaxFactory.EndOfLine(Environment.NewLine),
                    statementTrivia
                )
            );
        }

        return base.VisitToken(token);
    }
}
