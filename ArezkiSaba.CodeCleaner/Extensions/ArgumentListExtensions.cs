using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ArgumentListExtensions
{
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

    public static ArgumentListSyntax WithArgumentsPreservingTrivia(
        this ArgumentListSyntax argumentList,
        IEnumerable<ArgumentSyntax> arguments)
    {
        var existingArguments = argumentList.Arguments;
        var leadingTrivia = existingArguments.Select(arg => arg.GetLeadingTrivia()).ToList();
        var trailingTrivia = existingArguments.Select(arg => arg.GetTrailingTrivia()).ToList();

        var newArguments = SyntaxFactory.SeparatedList(arguments);

        for (int i = 0; i < newArguments.Count; i++)
        {
            newArguments = newArguments.Replace(
                newArguments[i],
                newArguments[i]
                    .WithLeadingTrivia(leadingTrivia.ElementAtOrDefault(i))
                    .WithTrailingTrivia(trailingTrivia.ElementAtOrDefault(i))
            );
        }

        // Return the new invocation expression with the updated arguments
        return SyntaxFactory.ArgumentList(newArguments);
    }
}
