using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class InvocationExpressionExtensions
{
    public static int GetExpressionLength(
        this ExpressionSyntax expression)
    {
        if (expression == null)
        {
            return 0;
        }

        var text = expression.GetText().ToString().Replace(Environment.NewLine, string.Empty);
        return text.Length;
    }
}