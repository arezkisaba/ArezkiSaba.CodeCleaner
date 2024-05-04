using Microsoft.CodeAnalysis.CSharp.Syntax;

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