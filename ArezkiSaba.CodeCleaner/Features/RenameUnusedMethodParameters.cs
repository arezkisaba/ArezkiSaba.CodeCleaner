using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class RenameUnusedMethodParameters
{
    public async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();

        var declarations = root.ChildNodes<BaseMethodDeclarationSyntax>(recursive: true);
        foreach (var declaration in declarations)
        {
            var parameters = declaration.DescendantNodes().OfType<ParameterSyntax>().ToList();
            if (parameters.Count != 2)
            {
                continue;
            }

            var senderParameterToken = parameters[0].DescendantTokens().OfType<SyntaxToken>()
                .LastOrDefault(obj => obj.ValueText == "sender");
            var argsParameterToken = parameters[1].DescendantTokens().OfType<SyntaxToken>()
                .FirstOrDefault(obj => obj.ValueText.EndsWith("Args"));

            var isCallbackMethod =
                !string.IsNullOrWhiteSpace(senderParameterToken.ValueText) &&
                !string.IsNullOrWhiteSpace(argsParameterToken.ValueText);

            if (!isCallbackMethod)
            {
                continue;
            }

            var senderParameter = parameters[0];
            solution = await solution.RenameParameterNameIfUnreferencedAsync(semanticModel, senderParameter, "_");
            var argsParameter = parameters[1];
            solution = await solution.RenameParameterNameIfUnreferencedAsync(semanticModel, argsParameter, "__");
        }

        return new RefactorOperationResult(
            solution.GetProject(document.Project.Id).GetDocument(document.Id),
            solution.GetProject(document.Project.Id),
            solution
        );
    }
}