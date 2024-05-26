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

        if (newEqualsValueClauseSyntax.Value is CollectionExpressionSyntax newCollectionExpression)
        {
            var i = 0;
            newEqualsValueClauseSyntax = newEqualsValueClauseSyntax.WithEqualsToken(
                newEqualsValueClauseSyntax.EqualsToken
                    .WithoutLeadingTrivia()
                    .WithEndOfLineTrivia()
            );

            newCollectionExpression = newCollectionExpression.WithOpenBracketToken(
                newCollectionExpression.OpenBracketToken
                    .WithIndentationTrivia(parentNode, indentCount: 0)
                    .WithEndOfLineTrivia()
            );
            newCollectionExpression = newCollectionExpression.ReplaceNodes(newCollectionExpression.Elements, (childElement, __) =>
            {
                if (i == newCollectionExpression.Elements.Count - 1)
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
            newCollectionExpression = newCollectionExpression.ReplaceTokens(
                newCollectionExpression.Elements.GetSeparators(), (childSeparator, __) =>
                {
                    return childSeparator
                        .WithoutLeadingTrivia()
                        .WithEndOfLineTrivia();
                }
            );
            newCollectionExpression = newCollectionExpression.WithCloseBracketToken(
                newCollectionExpression.CloseBracketToken
                    .WithIndentationTrivia(parentNode, indentCount: 0)
                    .WithoutTrailingTrivia()
            );

            newEqualsValueClauseSyntax = newEqualsValueClauseSyntax.WithValue(newCollectionExpression);
        }

        return newEqualsValueClauseSyntax;
    }
}
