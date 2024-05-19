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
}
