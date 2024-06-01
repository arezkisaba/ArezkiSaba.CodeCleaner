using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxNodeOrTokenExtensions
{
    public static SyntaxNodeOrToken WithEndOfLineTrivia(
        this SyntaxNodeOrToken item)
    {
        return item.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
    }

    public static IList<SyntaxTrivia> GetIndentation(
        this SyntaxNodeOrToken item,
        int indentationsToAdd = 0)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        var indentationTrivias = item.GetLeadingTrivia().Reverse().TakeWhile(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia)).ToList();
        leadingTrivias.AddRange(indentationTrivias);

        for (var j = 0; j < indentationsToAdd; j++)
        {
            leadingTrivias.Add(SyntaxTriviaHelper.GetTab());
        }

        return leadingTrivias;
    }

    public static int GetIndentationLevel(
        this SyntaxNodeOrToken item,
        int indentCount = 0)
    {
        return item.GetIndentationLength(indentCount) / Constants.IndentationCharacterCount;
    }

    public static int GetIndentationLength(
        this SyntaxNodeOrToken item,
        int indentCount = 0)
    {
        return item.GetIndentation(indentCount).Sum(obj => obj.FullSpan.Length);
    }

    public static bool IsStrictlyEqualTo(
        this SyntaxNodeOrToken item,
        SyntaxNodeOrToken compareTo)
    {
        return item.FullSpan == compareTo.FullSpan;
    }
}
