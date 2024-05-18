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
        var regionTrivias = token.GetAllTrivia().Where(
            obj => obj.IsKind(SyntaxKind.RegionDirectiveTrivia) || obj.IsKind(SyntaxKind.EndRegionDirectiveTrivia)
        ).ToList();
        if (!regionTrivias.Any())
        {
            return base.VisitToken(token);
        }

        return base.VisitToken(token.RemoveTrivias(GetTriviasToRemove(regionTrivias)));
    }

    #region Private use

    private static List<SyntaxTrivia> GetTriviasToRemove(
        IList<SyntaxTrivia> regionTrivias)
    {
        var triviasToRemove = new List<SyntaxTrivia>();
        for (var i = 0; i < regionTrivias.Count; i++)
        {
            var trivia = regionTrivias[i];
            triviasToRemove.Add(trivia);

            if (i > 0)
            {
                for (var j = i - 1; j >= 0; j--)
                {
                    var previousTrivia = regionTrivias[j];
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