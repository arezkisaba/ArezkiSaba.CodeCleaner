using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxTriviaHelper
{
    public static IList<SyntaxTrivia> GetLeadingTriviasBasedOn(
        SyntaxNode nodeBase,
        int indentCount = 0)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        var indentationTrivias = nodeBase.GetLeadingTrivia().Where(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia)).ToList();
        leadingTrivias.AddRange(indentationTrivias);

        for (var j = 0; j < indentCount; j++)
        {
            leadingTrivias.Add(SyntaxTriviaHelper.GetTab());
        }

        return leadingTrivias;
    }

    public static IList<SyntaxTrivia> GetLeadingTriviasBasedOn(
        SyntaxToken nodeBase,
        int indentCount = 0)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        var indentationTrivias = nodeBase.LeadingTrivia.Where(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia)).ToList();
        leadingTrivias.AddRange(indentationTrivias);

        for (var j = 0; j < indentCount; j++)
        {
            leadingTrivias.Add(SyntaxTriviaHelper.GetTab());
        }

        return leadingTrivias;
    }

    public static IList<SyntaxTrivia> GetArgumentLeadingTrivia(
        SyntaxTrivia? baseLeadingTrivia,
        int imbricationLevel)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        if (baseLeadingTrivia != null)
        {
            leadingTrivias.Add(baseLeadingTrivia.Value);
        }

        for (var j = 0; j < imbricationLevel + 1; j++)
        {
            leadingTrivias.Add(SyntaxTriviaHelper.GetTab());
        }

        return leadingTrivias;
    }

    public static SyntaxTrivia GetEndOfLine()
    {
        return SyntaxFactory.EndOfLine(Environment.NewLine);
    }

    public static SyntaxTrivia GetTab()
    {
        return GetWhitespace(4);
    }

    public static SyntaxTrivia GetWhitespace(
        int count = 1)
    {
        var whitespaces = StringHelper.GenerateCharacterOccurences(' ', count);
        return SyntaxFactory.Whitespace(whitespaces);
    }

    public static SyntaxTrivia GetRegion(
        string name)
    {
        return SyntaxFactory.Trivia(
            SyntaxFactory.RegionDirectiveTrivia(true)
                .WithTrailingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.Whitespace(" "),
                        SyntaxFactory.PreprocessingMessage(name)
                    )
                )
            );
    }

    public static SyntaxTrivia GetEndRegion()
    {
        return SyntaxFactory.Trivia(
            SyntaxFactory.EndRegionDirectiveTrivia(true)
        );
    }
}