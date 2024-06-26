﻿using Microsoft.CodeAnalysis.CSharp.Syntax;

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
