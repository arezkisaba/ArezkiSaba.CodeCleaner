using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq.Expressions;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ParenthesizedLambdaExpressionExtensions
{
    public static ParenthesizedLambdaExpressionSyntax Format(
        this ParenthesizedLambdaExpressionSyntax expression)
    {
        var block = expression.FirstChildNode<BlockSyntax>();
        return expression.WithBlock(
            block.IndentBlock(expression.GetLeadingTriviasCountBasedOn())
        );
    }
}
