using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxNodeOrTokenExtensions
{
    public static bool IsStrictlyEqualTo(
        this SyntaxNodeOrToken token,
        SyntaxNodeOrToken compareTo)
    {
        return token.FullSpan == compareTo.FullSpan;
    }
}
