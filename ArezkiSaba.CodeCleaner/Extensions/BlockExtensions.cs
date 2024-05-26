using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class BlockExtensions
{
    public static BlockSyntax AddTabLeadingTriviasOnBracesBasedOnParent(
        this BlockSyntax node,
        SyntaxNode parentNode)
    {
        var childTokens = node.ChildTokens();
        var newChildNode = node.ReplaceTokens(childTokens, (childToken, __) =>
        {
            if (childToken.IsKind(SyntaxKind.OpenBraceToken) || childToken.IsKind(SyntaxKind.CloseBraceToken))
            {
                return childToken
                    .WithIndentationTrivia(parentNode, indentCount: 0, keepOtherTrivias: true)
                    .WithEndOfLineTrivia();
            }

            return childToken;
        });
        return newChildNode;
    }

    public static BlockSyntax IndentBlock(
        this BlockSyntax block,
        int indentLevel)
    {
        var indent = new string(' ', indentLevel * Constants.IndentationCharacterCount);
        var indentedStatements = block.Statements
            .Select(statement => statement.IndentStatement(indentLevel + 1))
            .ToList();
        return block
            .WithOpenBraceToken(block.OpenBraceToken.WithLeadingTrivia(SyntaxFactory.Whitespace(indent)))
            .WithStatements(SyntaxFactory.List(indentedStatements))
            .WithCloseBraceToken(block.CloseBraceToken.WithLeadingTrivia(SyntaxFactory.Whitespace(indent)));
    }

    #region Private use

    private static StatementSyntax IndentStatement(
        this StatementSyntax statement,
        int indentLevel)
    {
        var indent = new string(' ', indentLevel * Constants.IndentationCharacterCount);
        if (statement is BlockSyntax block)
        {
            return IndentBlock(block, indentLevel);
        }

        var lines = statement.ToFullString().Split(new[] { Environment.NewLine }, StringSplitOptions.None)
            .Select(line => indent + line.TrimStart());
        var indentedText = string.Join(Environment.NewLine, lines);
        return SyntaxFactory.ParseStatement(indentedText);
    }

    #endregion
}
