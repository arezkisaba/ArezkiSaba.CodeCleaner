using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ExpressionSyntaxExtensions
{
    public static ArgumentListSyntax GetArgumentList(
        this ExpressionSyntax root)
    {
        if (root is InvocationExpressionSyntax invocationExpression)
        {
            return invocationExpression.ArgumentList;
        }
        else if (root is BaseObjectCreationExpressionSyntax objectCreationExpression)
        {
            return objectCreationExpression.ArgumentList;
        }

        throw new NotImplementedException($"ExpressionSyntax type not found : {root.GetType()}");
    }

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

    public static IList<AssignmentExpressionSyntax> GetAssignmentExpressions(
        this ExpressionSyntax root)
    {
        var initializerExpression = root.ChildNodes().OfType<InitializerExpressionSyntax>().FirstOrDefault();
        if (initializerExpression == null)
        {
            return Enumerable.Empty<AssignmentExpressionSyntax>().ToList();
        }

        return initializerExpression.ChildNodes().OfType<AssignmentExpressionSyntax>().ToList();
    }

    public static InitializerExpressionSyntax WithEndOfLines(
        this InitializerExpressionSyntax expression,
        IList<SyntaxTrivia> closeParenLeadingTrivia)
    {
        if (expression.Expressions.Count == expression.Expressions.GetSeparators().Count())
        {
            expression = expression.RemoveLastComma();
        }

        var i = 0;
        expression = expression.WithOpenBraceToken(expression.OpenBraceToken.WithLeadingTrivia(closeParenLeadingTrivia).WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine()));
        expression = expression.WithCloseBraceToken(expression.CloseBraceToken.WithLeadingTrivia(closeParenLeadingTrivia));
        expression = expression.ReplaceTokens(expression.Expressions.GetSeparators(), (separator, __) =>
        {
            return separator.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
        });

        expression = expression.ReplaceNodes(expression.Expressions, (argument, __) =>
        {
            if (i == expression.Expressions.Count - 1)
            {
                argument = argument.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
            }

            i++;
            return argument;
        });

        return expression;
    }

    private static InitializerExpressionSyntax RemoveLastComma(
        this InitializerExpressionSyntax initializer)
    {
        // Get the elements and separators
        var elements = initializer.Expressions;
        var separators = initializer.Expressions.GetSeparators().ToList();

        // If there are no separators, return the original initializer
        if (separators.Count == 0)
        {
            return initializer;
        }

        // Remove the last separator
        separators.RemoveAt(separators.Count - 1);

        // Create a new list of expressions interleaved with the modified separators
        var newExpressions = elements
            .Select((expr, index) => new { expr, index })
            .SelectMany(pair => pair.index < separators.Count
                ? new SyntaxNodeOrToken[] { pair.expr, separators[pair.index] }
                : new SyntaxNodeOrToken[] { pair.expr })
            .ToArray();

        // Return the new initializer expression
        return SyntaxFactory.InitializerExpression(initializer.Kind(), SyntaxFactory.SeparatedList<ExpressionSyntax>(newExpressions));
    }
}
