using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ConstructorDeclarationExtensions
{
    public static ConstructorDeclarationSyntax Format(
        this ConstructorDeclarationSyntax constructorDeclaration,
        ConstructorInitializerSyntax constructorInitializer,
        SyntaxNode parentNode)
    {
        var newConstructorInitializer = constructorInitializer.WithIndentationTrivia<ConstructorInitializerSyntax>(parentNode);
        return constructorDeclaration
            .WithParameterList(constructorDeclaration.ParameterList.WithEndOfLineTriviaAfterCloseParen())
            .WithInitializer(newConstructorInitializer);
    }
}
