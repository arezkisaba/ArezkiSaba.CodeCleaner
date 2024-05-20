using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxTokenExtensions
{
    public static bool IsEqualTo(
        this SyntaxToken root,
        SyntaxToken compareTo)
    {
        return root.FullSpan.Length == compareTo.FullSpan.Length;
    }

    public static SyntaxToken RemoveTrivias(
        this SyntaxToken root,
        IEnumerable<SyntaxTrivia> triviasToRemove)
    {
        var newTrivias = new SyntaxTriviaList(root.LeadingTrivia.Where(trivia => triviasToRemove.All(triviaToRemove => trivia != triviaToRemove)));
        return root.WithLeadingTrivia(newTrivias);
    }

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
