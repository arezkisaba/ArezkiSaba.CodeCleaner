using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using System.Reflection.Metadata;
using Newtonsoft.Json.Linq;

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
        var newInitializerExpression = initializerExpression;
        if (!newInitializerExpression.Expressions.Any())
        {
            return expression;
        }

        if (newInitializerExpression.Expressions.Count == newInitializerExpression.Expressions.GetSeparators().Count())
        {
            newInitializerExpression = newInitializerExpression.RemoveLastComma();
        }

        var i = 0;
        newInitializerExpression = newInitializerExpression.WithOpenBraceToken(
            newInitializerExpression.OpenBraceToken
                .WithIndentationTrivia(parentNode, indentCount)
                .WithEndOfLineTrivia()
        );
        newInitializerExpression = newInitializerExpression.ReplaceNodes(newInitializerExpression.Expressions, (childExpression, __) =>
        {
            if (i == newInitializerExpression.Expressions.Count - 1)
            {
                childExpression = childExpression.WithEndOfLineTrivia<ExpressionSyntax>();
            }

            i++;
            return childExpression.WithIndentationTrivia(parentNode, indentCount + 1);
        });
        newInitializerExpression = newInitializerExpression.ReplaceTokens(
            newInitializerExpression.Expressions.GetSeparators(), (separator, __) => separator.WithEndOfLineTrivia()
        );
        newInitializerExpression = newInitializerExpression.WithCloseBraceToken(
            newInitializerExpression.CloseBraceToken
                .WithIndentationTrivia(parentNode, indentCount)
        );

        var newKeyword = expression.ItemBefore(initializerExpression);
        expression = expression.WithEndOfLineTriviaAfter(newKeyword);
        return expression.WithInitializer(newInitializerExpression);
    }

    public static AnonymousObjectCreationExpressionSyntax Format(
        this AnonymousObjectCreationExpressionSyntax expression)
    {
        var newExpression = expression;
        if (!newExpression.Initializers.Any())
        {
            return expression;
        }

        if (newExpression.Initializers.Count == newExpression.Initializers.GetSeparators().Count())
        {
            newExpression = newExpression.RemoveLastComma();
        }

        var i = 0;
        newExpression = newExpression.WithOpenBraceToken(
            newExpression.OpenBraceToken
                .WithIndentationTrivia(newExpression, indentCount: 0)
                .WithEndOfLineTrivia()
        );
        newExpression = newExpression.ReplaceNodes(newExpression.Initializers, (childExpression, __) =>
        {
            if (i == newExpression.Initializers.Count - 1)
            {
                childExpression = childExpression.WithEndOfLineTrivia<AnonymousObjectMemberDeclaratorSyntax>();
            }

            i++;
            return childExpression.WithIndentationTrivia(newExpression, indentCount: 1);
        });
        newExpression = newExpression.ReplaceTokens(
            newExpression.Initializers.GetSeparators(), (separator, __) => separator.WithEndOfLineTrivia()
        );
        newExpression = newExpression.WithCloseBraceToken(
            newExpression.CloseBraceToken
                .WithIndentationTrivia(newExpression, indentCount: 0)
                .WithEndOfLineTrivia()
        );

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
