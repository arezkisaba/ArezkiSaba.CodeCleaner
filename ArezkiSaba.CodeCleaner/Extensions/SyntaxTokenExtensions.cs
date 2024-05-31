using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxTokenExtensions
{
    public static bool IsEqualTo(
        this SyntaxToken token,
        SyntaxToken compareTo)
    {
        return token.FullSpan.Length == compareTo.FullSpan.Length;
    }

    public static SyntaxToken WithIndentationTrivia(
        this SyntaxToken token,
        SyntaxNode parent,
        int indentCount = 1,
        bool keepOtherTrivias = false,
        bool mustAddLineBreakBefore = false)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        var indentationTrivia = parent.GetIndentation(indentCount);

        if (keepOtherTrivias)
        {
            leadingTrivias.AddRange(token.LeadingTrivia.Where(obj => !obj.IsKind(SyntaxKind.WhitespaceTrivia)));
        }

        if (mustAddLineBreakBefore)
        {
            leadingTrivias.Add(SyntaxTriviaHelper.GetEndOfLine());
        }

        leadingTrivias.AddRange(indentationTrivia);

        var newTriviaList = new List<SyntaxTrivia>();
        foreach (var trivia in leadingTrivias)
        {
            if (keepOtherTrivias &&
                (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) ||
                trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)))
            {
                newTriviaList.AddRange(indentationTrivia);
            }

            newTriviaList.Add(trivia);
        }

        return token.WithLeadingTrivia(newTriviaList);
    }

    public static SyntaxToken WithIndentationTrivia(
        this SyntaxToken token,
        SyntaxToken parent,
        int indentCount = 1,
        bool keepOtherTrivias = false,
        bool mustAddLineBreakBefore = false)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        var indentationTrivia = parent.GetIndentation(indentCount);

        if (keepOtherTrivias)
        {
            leadingTrivias.AddRange(token.LeadingTrivia.Where(obj => !obj.IsKind(SyntaxKind.WhitespaceTrivia)));
        }
        
        if (mustAddLineBreakBefore)
        {
            leadingTrivias.Add(SyntaxTriviaHelper.GetEndOfLine());
        }

        leadingTrivias.AddRange(indentationTrivia);

        var newTriviaList = new List<SyntaxTrivia>();
        foreach (var trivia in leadingTrivias)
        {
            if (keepOtherTrivias &&
                (trivia.IsKind(SyntaxKind.RegionDirectiveTrivia) ||
                trivia.IsKind(SyntaxKind.EndRegionDirectiveTrivia) ||
                trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)))
            {
                newTriviaList.AddRange(indentationTrivia);
            }

            newTriviaList.Add(trivia);
        }

        return token.WithLeadingTrivia(newTriviaList);
    }

    public static IList<SyntaxTrivia> GetIndentation(
        this SyntaxToken nodeBase,
        int indentationsToAdd = 0)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        var indentationTrivias = nodeBase.LeadingTrivia.Reverse().TakeWhile(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia)).ToList();
        leadingTrivias.AddRange(indentationTrivias);

        for (var j = 0; j < indentationsToAdd; j++)
        {
            leadingTrivias.Add(SyntaxTriviaHelper.GetTab());
        }

        return leadingTrivias;
    }

    public static SyntaxToken WithEndOfLineTrivia(
        this SyntaxToken token)
    {
        return token.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
    }

    public static SyntaxToken WithOrWithoutTrailingTriviaBasedOnNextItems(
        this SyntaxToken closeParentToken,
        SyntaxNode syntaxNode)
    {
        var directParentExpression = syntaxNode.FirstParentNode<ExpressionSyntax>();
        if (directParentExpression != null)
        {
            var syntaxTokenAfter = directParentExpression.Parent.ItemAfter(directParentExpression);
            if (syntaxTokenAfter.IsKind(SyntaxKind.CommaToken) || syntaxTokenAfter.IsKind(SyntaxKind.SemicolonToken))
            {
                closeParentToken = closeParentToken.WithoutTrailingTrivia();
            }
        }

        return closeParentToken;
    }

    public static SyntaxToken WithOrWithoutTrailingTriviaBasedOnNextItems2(
        this SyntaxToken syntaxToken,
        SyntaxNode relativeTo)
    {
        var tempLastParentItem = relativeTo;
        SyntaxToken syntaxTokenAfter = default;
        do
        {
            var lastParentItem = tempLastParentItem.FirstParentNode<SyntaxNode>();
            if (tempLastParentItem != null)
            {
                syntaxTokenAfter = lastParentItem.SyntaxTokenAfter(tempLastParentItem);
                if (syntaxTokenAfter.IsKind(SyntaxKind.CommaToken) || syntaxTokenAfter.IsKind(SyntaxKind.SemicolonToken))
                {
                    syntaxToken = syntaxToken.WithoutTrailingTrivia();
                }
                else
                {
                    syntaxToken = syntaxToken.WithEndOfLineTrivia();
                }
            }

            tempLastParentItem = lastParentItem;
        } while (syntaxTokenAfter.IsKind(SyntaxKind.None));

        return syntaxToken;
    }

    public static SyntaxToken RemoveTrivias(
        this SyntaxToken token,
        IEnumerable<SyntaxTrivia> triviasToRemove)
    {
        var newTrivias = new SyntaxTriviaList(token.LeadingTrivia.Where(trivia => triviasToRemove.All(triviaToRemove => trivia != triviaToRemove)));
        return token.WithLeadingTrivia(newTrivias);
    }

    public static SyntaxToken WithoutLeadingTrivia(
        this SyntaxToken token)
    {
        return token.WithLeadingTrivia();
    }

    public static SyntaxToken WithoutTrailingTrivia(
        this SyntaxToken token)
    {
        return token.WithTrailingTrivia();
    }

    public static SyntaxToken WithoutTrivia(
        this SyntaxToken token)
    {
        return token.WithoutLeadingTrivia().WithoutTrailingTrivia();
    }
}
