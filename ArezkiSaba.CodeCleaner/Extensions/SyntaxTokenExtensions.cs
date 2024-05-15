using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxTokenExtensions
{
    public static SyntaxToken WithoutLeadingTrivias(
        this SyntaxToken root)
    {
        return root.WithLeadingTrivia();
    }

    public static SyntaxToken WithoutTrailingTrivias(
        this SyntaxToken root)
    {
        return root.WithTrailingTrivia();
    }

    public static SyntaxToken WithoutTrivias(
        this SyntaxToken root)
    {
        return root.WithoutLeadingTrivias().WithoutTrailingTrivias();
    }
}
