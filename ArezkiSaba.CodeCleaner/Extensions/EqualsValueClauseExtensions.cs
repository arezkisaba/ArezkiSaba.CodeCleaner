using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class EqualsValueClauseExtensions
{
    public static EqualsValueClauseSyntax Format(
        this EqualsValueClauseSyntax equalsValueClauseSyntax,
        PropertyDeclarationSyntax parentNode,
        int indentCount)
    {
        if (parentNode == null)
        {
            return equalsValueClauseSyntax;
        }

        var newEqualsValueClauseSyntax = equalsValueClauseSyntax;

        var i = 0;
        newEqualsValueClauseSyntax = newEqualsValueClauseSyntax.WithEqualsToken(
            newEqualsValueClauseSyntax.EqualsToken
                .WithoutLeadingTrivia()
                .WithEndOfLineTrivia()
        );
        var newValue = newEqualsValueClauseSyntax.Value as CollectionExpressionSyntax;
        newValue = newValue.WithOpenBracketToken(
            newValue.OpenBracketToken
                .WithIndentationTrivia(parentNode, indentCount: 0)
                .WithEndOfLineTrivia()
        );
        newValue = newValue.ReplaceNodes(newValue.Elements, (childElement, __) =>
        {
            if (i == newValue.Elements.Count - 1)
            {
                childElement = childElement.WithEndOfLineTrivia<ExpressionElementSyntax>();
            }
            else
            {
                childElement = childElement.WithoutTrailingTrivia();
            }

            i++;
            return childElement.WithIndentationTrivia<ExpressionElementSyntax>(parentNode, indentCount + 1);
        });
        newValue = newValue.ReplaceTokens(
            newValue.Elements.GetSeparators(), (childSeparator, __) =>
            {
                return childSeparator
                    .WithoutLeadingTrivia()
                    .WithEndOfLineTrivia();
            }
        );
        newValue = newValue.WithCloseBracketToken(
            newValue.CloseBracketToken
                .WithIndentationTrivia(parentNode, indentCount: 0)
                .WithoutTrailingTrivia()
        );

        newEqualsValueClauseSyntax = newEqualsValueClauseSyntax.WithValue(newValue);
        return newEqualsValueClauseSyntax;
    }
}
