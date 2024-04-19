using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxTriviaHelper
{
    public static SyntaxTrivia GetEndOfLine()
    {
        return SyntaxFactory.EndOfLine(Environment.NewLine);
    }

    public static SyntaxTrivia GetTab()
    {
        return SyntaxFactory.Whitespace("    ");
    }
}