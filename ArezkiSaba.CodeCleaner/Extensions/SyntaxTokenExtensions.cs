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

    public static SyntaxToken WithEndOfLineTrivia(
        this SyntaxToken token)
    {
        return (SyntaxToken)((SyntaxNodeOrToken)token).WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
    }

    public static SyntaxToken WithIndentationTrivia(
        this SyntaxToken token,
        SyntaxNode relativeTo,
        int indentCount = 1,
        bool keepOtherTrivias = false,
        bool mustAddLineBreakBefore = false)
    {
        return token.WithIndentationTrivia((SyntaxNodeOrToken)relativeTo, indentCount, keepOtherTrivias, mustAddLineBreakBefore);
    }

    public static SyntaxToken WithIndentationTrivia(
        this SyntaxToken token,
        SyntaxToken relativeTo,
        int indentCount = 1,
        bool keepOtherTrivias = false,
        bool mustAddLineBreakBefore = false)
    {
        return token.WithIndentationTrivia((SyntaxNodeOrToken)relativeTo, indentCount, keepOtherTrivias, mustAddLineBreakBefore);
    }

    public static int GetIndentationLevel(
        this SyntaxToken item,
        int indentCount = 0)
    {
        return ((SyntaxNodeOrToken)item).GetIndentationLevel(indentCount);
    }

    public static int GetIndentationLength(
        this SyntaxToken item,
        int indentCount = 0)
    {
        return ((SyntaxNodeOrToken)item).GetIndentationLength(indentCount);
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

    public static SyntaxToken WithOrWithoutTrailingTriviaBasedOnNextItems(
        this SyntaxToken syntaxToken,
        CollectionExpressionSyntax relativeTo)
    {
        SyntaxNode tempLastParentItem = relativeTo;
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

    #region Private use

    private static SyntaxToken WithIndentationTrivia(
        this SyntaxToken token,
        SyntaxNodeOrToken relativeTo,
        int indentCount = 1,
        bool keepOtherTrivias = false,
        bool mustAddLineBreakBefore = false)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        var indentationTrivia = relativeTo.GetIndentation(indentCount);

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
            if (keepOtherTrivias && (trivia.IsRegionTrivia() || trivia.IsCommentTrivia()))
            {
                newTriviaList.AddRange(indentationTrivia);
            }

            newTriviaList.Add(trivia);
        }

        return token.WithLeadingTrivia(newTriviaList);
    }

    #endregion
}
