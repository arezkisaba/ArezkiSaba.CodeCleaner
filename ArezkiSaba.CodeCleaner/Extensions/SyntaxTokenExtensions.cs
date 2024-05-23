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

    public static SyntaxToken WithIndentationTrivia(
        this SyntaxToken node,
        SyntaxNode parentNode,
        int indentCount = 1)
    {
        return node.WithLeadingTrivia(SyntaxTriviaHelper.GetLeadingTriviasBasedOn(parentNode, indentCount));
    }

    public static SyntaxToken WithIndentationTrivia(
        this SyntaxToken node,
        SyntaxToken parentToken,
        int indentCount = 1)
    {
        return node.WithLeadingTrivia(SyntaxTriviaHelper.GetLeadingTriviasBasedOn(parentToken, indentCount));
    }

    public static SyntaxToken WithEndOfLineTrivia(
        this SyntaxToken root)
    {
        return root.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
    }

    public static SyntaxToken RemoveTrivias(
        this SyntaxToken root,
        IEnumerable<SyntaxTrivia> triviasToRemove)
    {
        var newTrivias = new SyntaxTriviaList(root.LeadingTrivia.Where(trivia => triviasToRemove.All(triviaToRemove => trivia != triviaToRemove)));
        return root.WithLeadingTrivia(newTrivias);
    }

    public static SyntaxToken WithoutLeadingTrivia(
        this SyntaxToken root)
    {
        return root.WithLeadingTrivia();
    }

    public static SyntaxToken WithoutTrailingTrivia(
        this SyntaxToken root)
    {
        return root.WithTrailingTrivia();
    }

    public static SyntaxToken WithoutTrivia(
        this SyntaxToken root)
    {
        return root.WithoutLeadingTrivia().WithoutTrailingTrivia();
    }
}
