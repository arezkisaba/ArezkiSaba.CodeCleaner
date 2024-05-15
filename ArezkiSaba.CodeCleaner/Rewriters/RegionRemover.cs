using ArezkiSaba.CodeCleaner.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ArezkiSaba.CodeCleaner.Rewriters;

public sealed class RegionRemover : CSharpSyntaxRewriter
{
    public override bool VisitIntoStructuredTrivia
    {
        get { return true; }
    }

    public override SyntaxTrivia VisitTrivia(
        SyntaxTrivia trivia)
    {
        if (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) ||
            trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
        {
            return SyntaxTriviaHelper.GetEndOfLine();
        }

        return base.VisitTrivia(trivia);
    }
}