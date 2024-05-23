using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ExpressionSyntaxExtensions
{
    public static ExpressionSyntax WithArgumentList(
        this ExpressionSyntax root,
        ArgumentListSyntax argumentList)
    {
        if (root is InvocationExpressionSyntax invocationExpression)
        {
            return invocationExpression.WithArgumentList(argumentList);
        }
        else if (root is BaseObjectCreationExpressionSyntax objectCreationExpression)
        {
            return objectCreationExpression.WithArgumentList(argumentList);
        }

        throw new NotImplementedException($"ExpressionSyntax type not found : {root.GetType()}");
    }

    public static ExpressionSyntax WithInitializer(
        this ExpressionSyntax root,
        InitializerExpressionSyntax initializerExpression)
    {
        if (root is BaseObjectCreationExpressionSyntax objectCreationExpression)
        {
            return objectCreationExpression.WithInitializer(initializerExpression);
        }
        else if (root is ImplicitArrayCreationExpressionSyntax implicitArrayCreationExpression)
        {
            return implicitArrayCreationExpression.WithInitializer(initializerExpression);
        }
        else if (root is ImplicitStackAllocArrayCreationExpressionSyntax implicitStackAllocArrayCreationExpression)
        {
            return implicitStackAllocArrayCreationExpression.WithInitializer(initializerExpression);
        }
        else if (root is ArrayCreationExpressionSyntax arrayCreationExpression)
        {
            return arrayCreationExpression.WithInitializer(initializerExpression);
        }
        else if (root is StackAllocArrayCreationExpressionSyntax stackAllocArrayCreationExpression)
        {
            return stackAllocArrayCreationExpression.WithInitializer(initializerExpression);
        }

        throw new NotImplementedException($"ExpressionSyntax type not found : {root.GetType()}");
    }

    public static ExpressionSyntax Format(
        this ExpressionSyntax expression,
        ArgumentListSyntax argumentList,
        SyntaxNode parentNode,
        int indentCount,
        bool isSpecialCase)
    {
        var needLineBreak = parentNode.GetLength() >= 100;
        if (!argumentList.Arguments.Any() || !needLineBreak)
        {
            return expression;
        }

        var i = 0;
        argumentList = argumentList.WithOpenParenToken(
            argumentList.OpenParenToken.WithEndOfLineTrivia()
        );
        argumentList = argumentList.ReplaceNodes(argumentList.Arguments, (childArgument, __) =>
        {
            if (i == argumentList.Arguments.Count - 1)
            {
                childArgument = childArgument.WithEndOfLineTrivia<ArgumentSyntax>();
            }

            i++;
            return childArgument.WithIndentationTrivia(parentNode, indentCount + 1);
        });
        argumentList = argumentList.ReplaceTokens(argumentList.Arguments.GetSeparators(), (separator, __) => separator.WithEndOfLineTrivia());

        var newCloseParentToken = argumentList.CloseParenToken.WithIndentationTrivia(parentNode, indentCount);
        if (isSpecialCase)
        {
            newCloseParentToken = newCloseParentToken.WithoutTrailingTrivia();
        }

        argumentList = argumentList.WithCloseParenToken(newCloseParentToken);
        return expression.WithArgumentList(argumentList);
    }

    public static ExpressionSyntax Format(
        this ExpressionSyntax expression,
        InitializerExpressionSyntax initializerExpression,
        SyntaxNode parentNode,
        int indentCount)
    {
        if (!initializerExpression.Expressions.Any())
        {
            return expression;
        }

        if (initializerExpression.Expressions.Count == initializerExpression.Expressions.GetSeparators().Count())
        {
            initializerExpression = initializerExpression.RemoveLastComma();
        }

        var i = 0;
        initializerExpression = initializerExpression.WithOpenBraceToken(
            initializerExpression.OpenBraceToken
                .WithIndentationTrivia(parentNode, indentCount)
                .WithEndOfLineTrivia()
        );
        initializerExpression = initializerExpression.ReplaceNodes(initializerExpression.Expressions, (childExpression, __) =>
        {
            if (i == initializerExpression.Expressions.Count - 1)
            {
                childExpression = childExpression.WithEndOfLineTrivia<ExpressionSyntax>();
            }

            i++;
            return childExpression.WithIndentationTrivia(parentNode, indentCount + 1);
        });
        initializerExpression = initializerExpression.ReplaceTokens(initializerExpression.Expressions.GetSeparators(), (separator, __) => separator.WithEndOfLineTrivia());
        initializerExpression = initializerExpression.WithCloseBraceToken(
            initializerExpression.CloseBraceToken
                .WithIndentationTrivia(parentNode, indentCount)
        );

        var itemBefore = expression.ItemBefore(initializerExpression);
        if (itemBefore.IsNode)
        {
            var targetNode = itemBefore.AsNode();
            var targetToken = targetNode.LastChildToken(recursive: true);
            expression = expression.ReplaceToken(targetToken, targetToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine()));
        }
        else if (itemBefore.IsToken)
        {
            var targetToken = itemBefore.AsToken();
            expression = expression.ReplaceToken(targetToken, targetToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine()));
        }

        return expression.WithInitializer(initializerExpression);
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
