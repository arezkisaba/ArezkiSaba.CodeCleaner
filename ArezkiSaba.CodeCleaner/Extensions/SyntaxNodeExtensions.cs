﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq.Expressions;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class SyntaxNodeExtensions
{
    public static IList<MethodDeclarationSyntax> FindAllMethods(
        this SyntaxNode root)
    {
        return root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
    }

    public static IList<BaseMethodDeclarationSyntax> FindAllConstructorAndMethodDeclarations(
        this SyntaxNode root)
    {
        return root.DescendantNodes().OfType<BaseMethodDeclarationSyntax>().ToList();
    }

    public static bool HasBaseOrThisInitializer(
        this SyntaxNode root)
    {
        return root.DescendantNodes().OfType<ConstructorInitializerSyntax>().Any();
    }

    public static SyntaxTrivia? FindFirstLeadingTrivia(
        this SyntaxNode root)
    {
        var baseLeadingTrivia = root.DescendantTrivia().FirstOrDefault(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));
        if (baseLeadingTrivia.IsKind(SyntaxKind.None))
        {
            return null;
        }

        return baseLeadingTrivia;
    }

    public static bool BeginsWithAutoGeneratedComment(
        this SyntaxNode root)
    {
        if (root.HasLeadingTrivia)
        {
            var commentTrivia = root.GetLeadingTrivia().Where(
                t => t.IsKind(SyntaxKind.MultiLineCommentTrivia) || t.IsKind(SyntaxKind.SingleLineCommentTrivia)
            );

            foreach (var trivia in commentTrivia)
            {
                var text = trivia.ToString().AsSpan();
                if (text.Contains("autogenerated", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("auto-generated", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
