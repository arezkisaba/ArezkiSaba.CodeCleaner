using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Xml.Linq;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxTokenExtensions
{
    public static bool IsEqualTo(
        this SyntaxToken token,
        SyntaxToken compareTo)
    {
        return token.FullSpan.Length == compareTo.FullSpan.Length;
    }

    public static SyntaxToken WithIndentationTrivia(
        this SyntaxToken token,
        SyntaxNode parentNode,
        int indentCount = 1,
        bool keepOtherTrivias = false)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        var indentationTrivia = SyntaxTriviaHelper.GetLeadingTriviasBasedOn(parentNode, indentCount);

        if (keepOtherTrivias)
        {
            leadingTrivias.AddRange(token.LeadingTrivia.Where(obj => !obj.IsKind(SyntaxKind.WhitespaceTrivia)));
        }

        leadingTrivias.AddRange(indentationTrivia);

        var newTriviaList = new List<SyntaxTrivia>();
        foreach (var trivia in leadingTrivias)
        {
            if (keepOtherTrivias &&
                !trivia.IsKind(SyntaxKind.EndOfLineTrivia) &&
                !trivia.IsKind(SyntaxKind.WhitespaceTrivia))
            {
                newTriviaList.AddRange(indentationTrivia);
            }

            newTriviaList.Add(trivia);
        }

        return token.WithLeadingTrivia(newTriviaList);
    }

    public static SyntaxToken WithEndOfLineTrivia(
        this SyntaxToken token)
    {
        return token.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
    }

    public static SyntaxToken RemoveTrivias(
        this SyntaxToken token,
        IEnumerable<SyntaxTrivia> triviasToRemove)
    {
        var newTrivias = new SyntaxTriviaList(token.LeadingTrivia.Where(trivia => triviasToRemove.All(triviaToRemove => trivia != triviaToRemove)));
        return token.WithLeadingTrivia(newTrivias);
    }

    public static SyntaxToken WithoutLeadingTrivia(
        this SyntaxToken token)
    {
        return token.WithLeadingTrivia();
    }

    public static SyntaxToken WithoutTrailingTrivia(
        this SyntaxToken token)
    {
        return token.WithTrailingTrivia();
    }

    public static SyntaxToken WithoutTrivia(
        this SyntaxToken token)
    {
        return token.WithoutLeadingTrivia().WithoutTrailingTrivia();
    }
}
