using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class AccessorDeclarationExtensions
{
    public static AccessorDeclarationSyntax Format(
        this AccessorDeclarationSyntax accessorDeclaration)
    {
        if (!accessorDeclaration.HasChildNode<BlockSyntax>())
        {
            return accessorDeclaration;
        }

        var newAccessorDeclaration = accessorDeclaration;
        var lastTokenBeforeCloseBrace = accessorDeclaration.ItemBefore(
            newAccessorDeclaration.LastChildToken(recursive: true),
            recursive: true
        ).AsToken();
        newAccessorDeclaration = newAccessorDeclaration.ReplaceToken(
            lastTokenBeforeCloseBrace,
            lastTokenBeforeCloseBrace.WithEndOfLineTrivia()
        );
        return newAccessorDeclaration.WithKeyword(accessorDeclaration.Keyword.WithEndOfLineTrivia());
    }
}
