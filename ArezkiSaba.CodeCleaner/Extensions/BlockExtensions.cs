using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class BlockExtensions
{
    public static SyntaxNode AddTabLeadingTriviasOnBracesBasedOnParent(
        this BlockSyntax node,
        SyntaxNode parentNode)
    {
        var childTokens = node.ChildTokens();
        var newChildNode = node.ReplaceTokens(childTokens, (childToken, __) =>
        {
            if (childToken.IsKind(SyntaxKind.OpenBraceToken) || childToken.IsKind(SyntaxKind.CloseBraceToken))
            {
                return childToken.WithLeadingTrivia(
                    SyntaxTriviaHelper.GetLeadingTriviasBasedOn(parentNode)
                );
            }

            return childToken;
        });
        return newChildNode;
    }

}
