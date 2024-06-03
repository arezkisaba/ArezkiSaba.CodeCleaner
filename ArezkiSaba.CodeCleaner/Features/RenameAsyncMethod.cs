using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class RenameAsyncMethod : RefactorOperationBase
{
    public override string Name => nameof(RenameAsyncMethod);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        if (document.IsEntryPoint())
        {
            return new RefactorOperationResult(
                document,
                document.Project,
                solution
            );
        }

        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var asyncSuffix = "Async";
        var declarations = root.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        foreach (var declaration in declarations)
        {
            var name = declaration.Identifier.ValueText;
            var hasAsyncSuffix = name.EndsWith(asyncSuffix);
            var hasAsyncKeyword = declaration.ChildTokens().Any(obj => obj.IsKind(SyntaxKind.AsyncKeyword));
            var hasTaskReturnTypeKeyword = declaration.ChildNodes().Any(node =>
            {
                return node.ChildTokens().Any(token => token.ValueText == "Task");
            });
            if (hasAsyncSuffix || !hasAsyncKeyword && !hasTaskReturnTypeKeyword)
            {
                continue;
            }

            var symbol = semanticModel.GetDeclaredSymbol(declaration);
            var newName = $"{declaration.Identifier.ValueText}{asyncSuffix}";
            solution = await solution.RenameSymbolAsync(symbol, name, newName);
            document = solution.GetProject(document.Project.Id).GetDocument(document.Id);
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }
}