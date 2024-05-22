using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ConstructorDeclarationExtensions
{
    public static ConstructorDeclarationSyntax FormatInitializer(
        this ConstructorDeclarationSyntax constructorDeclaration,
        ConstructorInitializerSyntax constructorInitializer,
        SyntaxNode parentNode)
    {
        var newConstructorInitializer = constructorInitializer.AddTabLeadingTriviasBasedOnParent(parentNode) as ConstructorInitializerSyntax;
        return constructorDeclaration
            .WithParameterList(constructorDeclaration.ParameterList.WithEndOfLineTriviaAfterCloseParen())
            .WithInitializer(newConstructorInitializer);
    }
}
