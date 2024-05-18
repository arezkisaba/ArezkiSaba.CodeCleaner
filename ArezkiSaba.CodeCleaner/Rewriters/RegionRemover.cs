using ArezkiSaba.CodeCleaner.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Rewriters;

public sealed class RegionRemover : CSharpSyntaxRewriter
{
    public override bool VisitIntoStructuredTrivia
    {
        get { return true; }
    }

    public override SyntaxToken VisitToken(
        SyntaxToken token)
    {
        var allTrivias = token.GetAllTrivia().ToList();
        var hasRegionTrivias = allTrivias.Any(
            obj => obj.IsKind(SyntaxKind.RegionDirectiveTrivia) || obj.IsKind(SyntaxKind.EndRegionDirectiveTrivia)
        );
        if (!hasRegionTrivias)
        {
            return base.VisitToken(token);
        }

        return base.VisitToken(token.RemoveTrivias(GetTriviasToRemove(allTrivias)));
    }

    #region Private use

    private static List<SyntaxTrivia> GetTriviasToRemove(
        IList<SyntaxTrivia> allTrivias)
    {
        var triviasToRemove = new List<SyntaxTrivia>();
        for (var i = 0; i < allTrivias.Count; i++)
        {
            var trivia = allTrivias[i];

            if (!trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) && !trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia))
            {
                continue;
            }

            triviasToRemove.Add(trivia);

            if (i > 0)
            {
                for (var j = i - 1; j >= 0; j--)
                {
                    var previousTrivia = allTrivias[j];
                    if (previousTrivia.IsKind(SyntaxKind.WhitespaceTrivia))
                    {
                        triviasToRemove.Add(previousTrivia);
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return triviasToRemove;
    }

    #endregion
}