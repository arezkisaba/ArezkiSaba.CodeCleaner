using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class RenameEventFields : RefactorOperationBase
{
    public override string Name => nameof(RenameEventFields);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var declarations = root.DescendantNodes().OfType<EventFieldDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            var name = declaration.GetName();
            if (string.IsNullOrWhiteSpace(name) || char.IsUpper(name[0]))
            {
                continue;
            }

            foreach (var variable in declaration.Declaration.Variables)
            {
                var symbol = semanticModel.GetDeclaredSymbol(variable);
                var newName = symbol.Name.ToPascalCase();
                solution = await solution.RenameSymbolAsync(symbol, name, newName);
            }
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }
}