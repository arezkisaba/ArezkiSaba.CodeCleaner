using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class RenameFields : RefactorOperationBase
{
    public override string Name => nameof(RenameFields);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var declarations = root.DescendantNodes().OfType<FieldDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            if (declaration.Modifiers.Any(m => m.IsKind(SyntaxKind.ConstKeyword) || m.IsKind(SyntaxKind.StaticKeyword)))
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
                    document = solution.GetProject(document.Project.Id).GetDocument(document.Id);
                }
            }
            else
            {
                var name = declaration.GetName();
                if (string.IsNullOrWhiteSpace(name) || name.StartsWith('_'))
                {
                    continue;
                }

                foreach (var variable in declaration.Declaration.Variables)
                {
                    var symbol = semanticModel.GetDeclaredSymbol(variable);
                    var newName = $"_{name}";
                    solution = await solution.RenameSymbolAsync(symbol, name, newName);
                }
            }
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }
}