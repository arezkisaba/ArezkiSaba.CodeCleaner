using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq.Expressions;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class InitializerExpressionExtensions
{
    public static ExpressionSyntax Format(
        this ExpressionSyntax expression,
        InitializerExpressionSyntax initializerExpression,
        int indentCount)
    {
        if (initializerExpression.Expressions.Count == initializerExpression.Expressions.GetSeparators().Count())
        {
            initializerExpression = initializerExpression.RemoveLastComma();
        }

        var i = 0;
        initializerExpression = initializerExpression.WithOpenBraceToken(
            initializerExpression.OpenBraceToken
                .WithIndentationTrivia(expression, indentCount: 0)
                .WithEndOfLineTrivia()
        );
        initializerExpression = initializerExpression.ReplaceNodes(initializerExpression.Expressions, (childExpression, __) =>
        {
            if (i == initializerExpression.Expressions.Count - 1)
            {
                childExpression = childExpression.WithEndOfLineTrivia<ExpressionSyntax>();
            }

            i++;
            return childExpression.WithIndentationTrivia(expression);
        });
        initializerExpression = initializerExpression.ReplaceTokens(initializerExpression.Expressions.GetSeparators(), (separator, __) => separator.WithEndOfLineTrivia());
        initializerExpression = initializerExpression.WithCloseBraceToken(
            initializerExpression.CloseBraceToken
                .WithIndentationTrivia(expression, indentCount: 0)
                .WithEndOfLineTrivia()
        );

        var newExpression = expression.WithInitializer(initializerExpression);
        var itemBefore = newExpression.ItemBefore(initializerExpression);
        if (itemBefore.IsNode)
        {
            var targetNode = itemBefore.AsNode();
            var targetToken = targetNode.LastChildToken(recursive: true);
            newExpression = newExpression.ReplaceToken(targetToken, targetToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine()));
        }
        else if (itemBefore.IsToken)
        {
            var targetToken = itemBefore.AsToken();
            newExpression = newExpression.ReplaceToken(targetToken, targetToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine()));
        }

        return newExpression as ExpressionSyntax;
    }

    #region Private use

    private static InitializerExpressionSyntax RemoveLastComma(
        this InitializerExpressionSyntax initializer)
    {
        var elements = initializer.Expressions;
        var separators = initializer.Expressions.GetSeparators().ToList();

        if (separators.Count == 0)
        {
            return initializer;
        }

        separators.RemoveAt(separators.Count - 1);

        var newExpressions = elements
            .Select((expr, index) => new { expr, index })
            .SelectMany(pair => pair.index < separators.Count
                ? new SyntaxNodeOrToken[] { pair.expr, separators[pair.index] }
                : new SyntaxNodeOrToken[] { pair.expr })
            .ToArray();

        return SyntaxFactory.InitializerExpression(initializer.Kind(), SyntaxFactory.SeparatedList<ExpressionSyntax>(newExpressions));
    }

    #endregion
}
