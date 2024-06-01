using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class AssignmentExpressionExtensions
{
    public static AssignmentExpressionSyntax Format(
        this AssignmentExpressionSyntax assignmentExpression)
    {
        var collectionExpression = assignmentExpression.FirstChildNode<CollectionExpressionSyntax>();
        if (collectionExpression != null)
        {
            assignmentExpression = assignmentExpression.HandleCollectionExpression(collectionExpression);
        }

        return assignmentExpression;
    }

    #region Private use

    private static AssignmentExpressionSyntax HandleCollectionExpression(
        this AssignmentExpressionSyntax assignmentExpression,
        CollectionExpressionSyntax collectionExpression)
    {
        var newCollectionExpression = collectionExpression;
        if (newCollectionExpression.Elements.Any())
        {
            var i = 0;
            assignmentExpression = assignmentExpression.WithOperatorToken(
                assignmentExpression.OperatorToken
                    .WithoutLeadingTrivia()
                    .WithEndOfLineTrivia()
            );
            newCollectionExpression = newCollectionExpression.WithOpenBracketToken(
                newCollectionExpression.OpenBracketToken
                    .WithIndentationTrivia(assignmentExpression, indentCount: 0)
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
                return childElement.WithIndentationTrivia<ExpressionElementSyntax>(newCollectionExpression, indentCount: 1);
            });
            newCollectionExpression = newCollectionExpression.ReplaceTokens(
                newCollectionExpression.Elements.GetSeparators(), (childSeparator, __) =>
                {
                    return childSeparator
                        .WithoutLeadingTrivia()
                        .WithEndOfLineTrivia();
                }
            );

            var closeBracketToken = newCollectionExpression.CloseBracketToken
                .WithIndentationTrivia(
                    assignmentExpression,
                    indentCount : 0
                );
            closeBracketToken = closeBracketToken.WithOrWithoutTrailingTriviaBasedOnNextItems(collectionExpression);
            newCollectionExpression = newCollectionExpression.WithCloseBracketToken(closeBracketToken);
            return assignmentExpression.WithRight(newCollectionExpression);
        }
        else
        {
            assignmentExpression = assignmentExpression.WithOperatorToken(
                assignmentExpression.OperatorToken
                    .WithoutLeadingTrivia()
                    .WithTrailingTrivia(SyntaxTriviaHelper.GetWhitespace())
            );
            newCollectionExpression = newCollectionExpression.WithOpenBracketToken(
                newCollectionExpression.OpenBracketToken
                    .WithoutLeadingTrivia()
                    .WithoutTrailingTrivia()
            );

            var closeBracketToken = newCollectionExpression.CloseBracketToken
                .WithoutLeadingTrivia();
            closeBracketToken = closeBracketToken.WithOrWithoutTrailingTriviaBasedOnNextItems(collectionExpression);
            newCollectionExpression = newCollectionExpression.WithCloseBracketToken(closeBracketToken);
            return assignmentExpression.WithRight(newCollectionExpression);
        }
    }

    #endregion
}
