using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxTriviaHelper
{
    public static int GetImbricationLevel(
        ExpressionSyntax expression)
    {
        var imbricationLevel = 0;
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

    public static IList<SyntaxTrivia> GetCloseBraceOrParenLeadingTrivia(
        SyntaxTrivia? baseLeadingTrivia,
        int imbricationLevel)
    {
        var closeBraceLeadingTrivia = new List<SyntaxTrivia>();
        if (baseLeadingTrivia != null)
        {
            closeBraceLeadingTrivia.Add(baseLeadingTrivia.Value);
        }

        for (var j = 0; j < imbricationLevel; j++)
        {
            closeBraceLeadingTrivia.Add(SyntaxTriviaHelper.GetTab());
        }

        return closeBraceLeadingTrivia;
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