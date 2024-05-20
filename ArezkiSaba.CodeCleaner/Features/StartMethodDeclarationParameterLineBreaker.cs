using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class StartMethodDeclarationParameterLineBreaker
{
    public async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var declarations = documentEditor.GetDocumentEditorRoot().ChildNodes<BaseMethodDeclarationSyntax>(recursive: true);
        foreach (var declaration in declarations)
        {
            var baseLeadingTrivia = declaration.FindFirstLeadingTrivia();
            if (baseLeadingTrivia == null)
            {
                continue;
            }

            var needLineBreak = true;
            var parametersList = declaration.ParameterList;
            var newParameters = new List<ParameterSyntax>();
            for (var i = 0; i < parametersList.Parameters.Count; i++)
            {
                var parameter = parametersList.Parameters[i];
                if (needLineBreak)
                {
                    parameter = parameter.WithLeadingTrivia(
                        SyntaxTriviaHelper.GetEndOfLine(),
                        baseLeadingTrivia.Value,
                        SyntaxTriviaHelper.GetTab()
                    );
                }

                newParameters.Add(parameter);
            }

            var newParametersList = parametersList.WithParameters(
                SyntaxFactory.SeparatedList(newParameters)
            );
            var newMethodDeclaration = declaration.WithParameterList(newParametersList);
            documentEditor.ReplaceNode(declaration, newMethodDeclaration);
        }

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}