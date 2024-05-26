using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ExpressionSyntaxExtensions
{
    public static ExpressionSyntax WithArgumentList(
        this ExpressionSyntax expression,
        ArgumentListSyntax argumentList)
    {
        if (expression is InvocationExpressionSyntax invocationExpression)
        {
            return invocationExpression.WithArgumentList(argumentList);
        }
        else if (expression is BaseObjectCreationExpressionSyntax objectCreationExpression)
        {
            return objectCreationExpression.WithArgumentList(argumentList);
        }

        throw new NotImplementedException($"ExpressionSyntax type not found : {expression.GetType()}");
    }

    public static ExpressionSyntax WithInitializer(
        this ExpressionSyntax expression,
        InitializerExpressionSyntax initializerExpression)
    {
        if (expression is BaseObjectCreationExpressionSyntax objectCreationExpression)
        {
            return objectCreationExpression.WithInitializer(initializerExpression);
        }
        else if (expression is ImplicitArrayCreationExpressionSyntax implicitArrayCreationExpression)
        {
            return implicitArrayCreationExpression.WithInitializer(initializerExpression);
        }
        else if (expression is ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAllocArrayCreationExpression)
        {
            return implicitStackAllocArrayCreationExpression.WithInitializer(initializerExpression);
        }
        else if (expression is ArrayCreationExpressionSyntax arrayCreationExpression)
        {
            return arrayCreationExpression.WithInitializer(initializerExpression);
        }
        else if (expression is StackAllocArrayCreationExpressionSyntax stackAllocArrayCreationExpression)
        {
            return stackAllocArrayCreationExpression.WithInitializer(initializerExpression);
        }

        throw new NotImplementedException($"ExpressionSyntax type not found : {expression.GetType()}");
    }

    public static ExpressionSyntax Format(
        this ExpressionSyntax expression,
        ArgumentListSyntax argumentList,
        StatementSyntax parentStatement,
        int indentCount)
    {
        var needLineBreak = expression.GetLength() >= 100;
        if (!argumentList.Arguments.Any() || parentStatement == null)
        {
            return expression;
        }

        var newArgumentList = argumentList;
        if (needLineBreak)
        {
            var i = 0;
            newArgumentList = newArgumentList.WithOpenParenToken(
                newArgumentList.OpenParenToken
                    .WithoutLeadingTrivia()
                    .WithEndOfLineTrivia()
            );
            newArgumentList = newArgumentList.ReplaceNodes(newArgumentList.Arguments, (childArgument, __) =>
            {
                if (i == newArgumentList.Arguments.Count - 1)
                {
                    childArgument = childArgument.WithEndOfLineTrivia<ArgumentSyntax>();
                }
                else
                {
                    childArgument = childArgument.WithoutTrailingTrivia();
                }

                i++;
                return childArgument.WithIndentationTrivia<ArgumentSyntax>(parentStatement, indentCount + 1);
            });
            newArgumentList = newArgumentList.ReplaceTokens(newArgumentList.Arguments.GetSeparators(), (childSeparator, __) =>
            {
                return childSeparator
                    .WithoutLeadingTrivia()
                    .WithEndOfLineTrivia();
            });

            var closeParentToken = newArgumentList.CloseParenToken
                .WithIndentationTrivia(
                    parentStatement,
                    indentCount
                );
            closeParentToken = closeParentToken.WithOrWithoutTrailingTriviaBasedOnNextItems(argumentList);

            newArgumentList = newArgumentList.WithCloseParenToken(closeParentToken);
        }
        else
        {
            newArgumentList = newArgumentList.WithOpenParenToken(
                newArgumentList.OpenParenToken
                    .WithoutLeadingTrivia()
                    .WithoutTrailingTrivia()
            );
            newArgumentList = newArgumentList.ReplaceNodes(newArgumentList.Arguments, (childArgument, __) =>
            {
                return childArgument
                    .WithoutLeadingTrivia()
                    .WithoutTrailingTrivia();
            });
            newArgumentList = newArgumentList.ReplaceTokens(newArgumentList.Arguments.GetSeparators(), (childSeparator, __) =>
            {
                return childSeparator
                    .WithoutLeadingTrivia()
                    .WithTrailingTrivia(SyntaxTriviaHelper.GetWhitespace());
            });

            var closeParentToken = newArgumentList.CloseParenToken
                .WithoutLeadingTrivia();
            closeParentToken = closeParentToken.WithOrWithoutTrailingTriviaBasedOnNextItems(argumentList);

            newArgumentList = newArgumentList.WithCloseParenToken(closeParentToken);
        }

        return expression.WithArgumentList(newArgumentList);
    }

    public static ExpressionSyntax Format(
        this ExpressionSyntax expression,
        InitializerExpressionSyntax initializerExpression,
        StatementSyntax parentStatement,
        int indentCount)
    {
        var needLineBreak = true;
        if (parentStatement == null)
        {
            return expression;
        }

        var newInitializerExpression = initializerExpression;
        if (newInitializerExpression.Expressions.Count == newInitializerExpression.Expressions.GetSeparators().Count())
        {
            newInitializerExpression = newInitializerExpression.RemoveLastComma();
        }

        if (needLineBreak)
        {
            var i = 0;
            newInitializerExpression = newInitializerExpression.WithOpenBraceToken(
                newInitializerExpression.OpenBraceToken
                    .WithIndentationTrivia(parentStatement, indentCount)
                    .WithEndOfLineTrivia()
            );
            newInitializerExpression = newInitializerExpression.ReplaceNodes(newInitializerExpression.Expressions, (childExpression, __) =>
            {
                if (i == newInitializerExpression.Expressions.Count - 1)
                {
                    childExpression = childExpression.WithEndOfLineTrivia<ExpressionSyntax>();
                }
                else
                {
                    childExpression = childExpression.WithoutTrailingTrivia();
                }

                i++;
                return childExpression.WithIndentationTrivia<ExpressionSyntax>(parentStatement, indentCount + 1);
            });
            newInitializerExpression = newInitializerExpression.ReplaceTokens(
                newInitializerExpression.Expressions.GetSeparators(), (childSeparator, __) =>
                {
                    return childSeparator
                        .WithoutLeadingTrivia()
                        .WithEndOfLineTrivia();
                }
            );
            newInitializerExpression = newInitializerExpression.WithCloseBraceToken(
                newInitializerExpression.CloseBraceToken
                    .WithIndentationTrivia(
                        parentStatement,
                        indentCount
                    )
                    .WithoutTrailingTrivia()
            );
        }

        var newKeyword = expression.ItemBefore(initializerExpression);
        return expression.WithEndOfLineTriviaAfter(newKeyword).WithInitializer(newInitializerExpression);
    }

    public static AnonymousObjectCreationExpressionSyntax Format(
        this AnonymousObjectCreationExpressionSyntax expression,
        SyntaxNode parentNode,
        int indentCount)
    {
        var needLineBreak = true;
        if (parentNode == null)
        {
            return expression;
        }

        var newExpression = expression;
        if (newExpression.Initializers.Count == newExpression.Initializers.GetSeparators().Count())
        {
            newExpression = newExpression.RemoveLastComma();
        }

        if (needLineBreak)
        {
            newExpression = newExpression.WithOpenBraceToken(
                newExpression.OpenBraceToken
                    .WithIndentationTrivia(parentNode, indentCount)
                    .WithEndOfLineTrivia()
            );
            newExpression = newExpression.ReplaceNodes(newExpression.Initializers, (childExpression, __) =>
            {
                return childExpression
                    .WithIndentationTrivia(parentNode, indentCount + 1)
                    .WithoutTrailingTrivia();
            });
            newExpression = newExpression.ReplaceTokens(
                newExpression.Initializers.GetSeparators(), (childSeparator, __) =>
                {
                    return childSeparator
                        .WithoutLeadingTrivia()
                        .WithEndOfLineTrivia();
                }
            );
            newExpression = newExpression.WithCloseBraceToken(
                newExpression.CloseBraceToken.WithIndentationTrivia(
                    parentNode,
                    indentCount,
                    mustAddLineBreakBefore: newExpression.Initializers.Count > 0
                )
            );
        }

        var newKeyword = newExpression.ItemBefore(newExpression.OpenBraceToken);
        return newExpression.WithEndOfLineTriviaAfter(newKeyword);
    }

    #region Private use

    private static T WithEndOfLineTriviaAfter<T>(
        this T node,
        SyntaxNodeOrToken after)
        where T : SyntaxNode
    {
        if (after.IsNode)
        {
            var targetNode = after.AsNode();
            var targetToken = targetNode.LastChildToken(recursive: true);
            node = node.ReplaceToken(targetToken, targetToken.WithEndOfLineTrivia());
        }
        else if (after.IsToken)
        {
            var targetToken = after.AsToken();
            node = node.ReplaceToken(targetToken, targetToken.WithEndOfLineTrivia());
        }

        return node as T;
    }

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

    private static AnonymousObjectCreationExpressionSyntax RemoveLastComma(
        this AnonymousObjectCreationExpressionSyntax anonymousObjectCreationExpression)
    {
        var initializers = anonymousObjectCreationExpression.Initializers;
        var separators = anonymousObjectCreationExpression.Initializers.GetSeparators().ToList();

        if (separators.Count == 0)
        {
            return anonymousObjectCreationExpression;
        }

        var newInitializers = SyntaxFactory.SeparatedList(
            initializers.Take(initializers.Count),
            separators.Take(separators.Count - 1)
        );

        return anonymousObjectCreationExpression.WithInitializers(newInitializers);
    }

    #endregion
}
