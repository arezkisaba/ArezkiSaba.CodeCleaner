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

    public override SyntaxToken VisitToken(
        SyntaxToken token)
    {
        var regionsDirectives = new List<SyntaxKind>()
        {
            SyntaxKind.RegionDirectiveTrivia,
            SyntaxKind.EndRegionDirectiveTrivia
        };
        var allTrivias = token.GetAllTrivia().ToList();
        var hasRegionTrivias = allTrivias.Any(trivia => regionsDirectives.Any(regionDirective => trivia.IsKind(regionDirective)));
        if (!hasRegionTrivias)
        {
            return base.VisitToken(token);
        }

        return base.VisitToken(token.RemoveTrivias(GetTriviasToRemove(allTrivias, regionsDirectives)));
    }

    #region Private use

    private static List<SyntaxTrivia> GetTriviasToRemove(
        IList<SyntaxTrivia> allTrivias,
        IList<SyntaxKind> regionsDirectives)
    {
        var triviasToRemove = new List<SyntaxTrivia>();
        for (var i = 0; i < allTrivias.Count; i++)
        {
            var trivia = allTrivias[i];
            if (regionsDirectives.All(regionDirective => !trivia.IsKind(regionDirective)))
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