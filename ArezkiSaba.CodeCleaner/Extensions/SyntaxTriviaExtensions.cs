using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxTriviaExtensions
{
    public static bool IsCommentTrivia(
        this SyntaxTrivia trivia)
    {
        var commentTriviaKinds = new[]
        {
            SyntaxKind.SingleLineCommentTrivia,
            SyntaxKind.MultiLineCommentTrivia,
            SyntaxKind.DocumentationCommentExteriorTrivia,
            SyntaxKind.MultiLineDocumentationCommentTrivia,
            SyntaxKind.SingleLineDocumentationCommentTrivia
        };

        return commentTriviaKinds.Any(kind => trivia.IsKind(kind));
    }

    public static bool IsRegionTrivia(
        this SyntaxTrivia trivia)
    {
        var regionTriviaKinds = new[]
        {
            SyntaxKind.RegionDirectiveTrivia,
            SyntaxKind.EndRegionDirectiveTrivia
        };

        return regionTriviaKinds.Any(kind => trivia.IsKind(kind));
    }
}
