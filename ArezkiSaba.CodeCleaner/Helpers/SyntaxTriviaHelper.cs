using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxTriviaHelper
{
    public static IList<SyntaxTrivia> GetLeadingTriviasBasedOn(
        SyntaxNode nodeBase,
        int indentCount = 0)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        var indentationTrivias = nodeBase.GetLeadingTrivia().Reverse().TakeWhile(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia)).ToList();
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

    public static int GetImbricationLevel(
        ExpressionSyntax expression,
        bool isSpecialCase = false)
    {
        var imbricationLevel = 0;

        if (!isSpecialCase)
        {
            var ancestors = expression.Ancestors().ToList();
            foreach (var ancestor in ancestors)
            {
                if (ancestor.IsImbricationExpression())
                {
                    imbricationLevel++;
                }
                else if (ancestor is StatementSyntax)
                {
                    break;
                }
            }
        }

        return imbricationLevel;
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
        return SyntaxFactory.Whitespace("    ");
    }

    public static SyntaxTrivia GetWhitespace()
    {
        return SyntaxFactory.Whitespace(" ");
    }
}