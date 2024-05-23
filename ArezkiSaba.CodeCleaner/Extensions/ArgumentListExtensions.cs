using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ArgumentListExtensions
{
    public static ArgumentListSyntax Format(
        this ArgumentListSyntax argumentList,
        SyntaxNode parentNode,
        int indentCount)
    {
        var i = 0;
        argumentList = argumentList.WithOpenParenToken(argumentList.OpenParenToken.WithEndOfLineTrivia());
        argumentList = argumentList.ReplaceNodes(argumentList.Arguments, (childArgument, __) =>
        {
            if (i == argumentList.Arguments.Count - 1)
            {
                childArgument = childArgument.WithEndOfLineTrivia<ArgumentSyntax>();
            }

            i++;
            return childArgument.WithLeadingTrivia(
                SyntaxTriviaHelper.GetLeadingTriviasBasedOn(parentNode, indentCount + 1)
            );
        });
        argumentList = argumentList.ReplaceTokens(argumentList.Arguments.GetSeparators(), (separator, __) => separator.WithEndOfLineTrivia());
        argumentList = argumentList.WithCloseParenToken(argumentList.CloseParenToken.WithIndentationTrivia(parentNode, indentCount: indentCount));
        return argumentList;
    }

    public static ArgumentListSyntax WithEndOfLines(
        this ArgumentListSyntax argumentList,
        IList<SyntaxTrivia> closeParenLeadingTrivia)
    {
        var i = 0;
        argumentList = argumentList.WithOpenParenToken(argumentList.OpenParenToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine()));
        argumentList = argumentList.WithCloseParenToken(argumentList.CloseParenToken.WithLeadingTrivia(closeParenLeadingTrivia));
        argumentList = argumentList.ReplaceTokens(argumentList.Arguments.GetSeparators(), (separator, __) =>
        {
            return separator.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
        });
        argumentList = argumentList.ReplaceNodes(argumentList.Arguments, (argument, __) =>
        {
            if (i == argumentList.Arguments.Count - 1)
            {
                argument = argument.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
            }

            i++;
            return argument;
        });

        return argumentList;
    }
}
