using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class RenameProperties : RefactorOperationBase
{
    public override string Name => nameof(RenameProperties);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        if (document.Name.Contains("ViewModel"))
        {
            return new RefactorOperationResult(
                document,
                document.Project,
                solution
            );
        }

        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var declarations = root.DescendantNodes().OfType<PropertyDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            var name = declaration.GetName();
            if (string.IsNullOrWhiteSpace(name) || char.IsUpper(name[0]))
            {
                continue;
            }

            var symbol = semanticModel.GetDeclaredSymbol(declaration);
            var newName = symbol.Name.ToPascalCase();
            solution = await solution.RenameSymbolAsync(symbol, name, newName);
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }
}