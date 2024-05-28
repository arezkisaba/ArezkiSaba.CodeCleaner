using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq.Expressions;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ParenthesizedLambdaExpressionExtensions
{
    public static LambdaExpressionSyntax Format(
        this LambdaExpressionSyntax expression)
    {
        var block = expression.FirstChildNode<BlockSyntax>();
        if (block != null)
        {
            return expression.WithBlock(
                block.IndentBlock(expression.GetIndentationLevel())
            );
        }

        return expression;
    }
}
