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
        if (token.Parent is not ParameterListSyntax parameterList ||
            token.Parent.Ancestors().OfType<LocalFunctionStatementSyntax>().Any() ||
            token.Parent.Ancestors().OfType<ParenthesizedLambdaExpressionSyntax>().Any() ||
            !parameterList.Parameters.Any())
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

        var statement = token.Parent.Ancestors().FirstOrDefault(
            obj => obj.IsKind(SyntaxKind.ConstructorDeclaration) || obj.IsKind(SyntaxKind.MethodDeclaration)
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

        return base.VisitToken(token);
    }
}
