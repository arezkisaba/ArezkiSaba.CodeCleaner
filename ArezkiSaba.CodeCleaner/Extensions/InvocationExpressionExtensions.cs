using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq.Expressions;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class InvocationExpressionExtensions
{
    public static int GetInvocationExpressionLength(
        this InvocationExpressionSyntax invocationExpression)
    {
        if (invocationExpression == null)
        {
            return 0;
        }

        var text = invocationExpression.GetText().ToString();
        return text.Length;
    }
}