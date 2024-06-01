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
        var indent = StringHelper.GenerateCharacterOccurences(' ', indentLevel * Constants.IndentationCharacterCount);
        var indentedStatements = block.Statements
            .Select((statement, index) => statement.IndentStatement(indentLevel + 1, index == block.Statements.Count - 1))
            .ToList();
        return block
            .WithOpenBraceToken(block.OpenBraceToken.WithLeadingTrivia(SyntaxFactory.Whitespace(indent)))
            .WithStatements(SyntaxFactory.List(indentedStatements))
            .WithCloseBraceToken(block.CloseBraceToken.WithLeadingTrivia(SyntaxFactory.Whitespace(indent)));
    }

    #region Private use

    private static StatementSyntax IndentStatement(
        this StatementSyntax statement,
        int indentLevel,
        bool isLastStatement)
    {
        var indent = new string(' ', indentLevel * Constants.IndentationCharacterCount);
        if (statement is BlockSyntax block)
        {
            return IndentBlock(block, indentLevel);
        }

        var minIndent = 0;
        var allLines = statement.ToFullString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        foreach (var line in allLines)
        {
            var i = 0;
            foreach (var car in line)
            {
                if (car != ' ')
                {
                    if (minIndent == 0 || i < minIndent)
                    {
                        minIndent = i;
                    }

                    break;
                }

                i++;
            }
        }

        var newLines = new List<string>();
        foreach (var line in allLines)
        {
            var sanitizedLine = minIndent < line.Length ? line.Substring(minIndent, line.Length - minIndent) : line;
            newLines.Add(indent + sanitizedLine);
        }

        var indentedText = string.Join(Environment.NewLine, newLines);
        if (isLastStatement && !indentedText.EndsWith(Environment.NewLine))
        {
            indentedText = $"{indentedText}{Environment.NewLine}";
        }

        return SyntaxFactory.ParseStatement(indentedText);
    }

    #endregion
}
