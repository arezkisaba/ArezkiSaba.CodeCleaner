using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class AddPrivateUseRegion
{
    public async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        if (document.IsEntryPoint())
        {
            return new RefactorOperationResult(
                document,
                document.Project,
                document.Project.Solution
            );
        }

        var documentEditor = await DocumentEditor.CreateAsync(document);
        var classDeclarations = documentEditor.OriginalRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().ToList();
        foreach (var classDeclaration in classDeclarations)
        {
            var methodDeclarations = classDeclaration.ChildNodes().OfType<MethodDeclarationSyntax>()
                .Where(obj => obj.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
                .ToList();
            if (!methodDeclarations.Any())
            {
                continue;
            }

            var firstDeclaration = methodDeclarations.First();
            var firstPrivateMethodLeadingTrivias = new List<SyntaxTrivia>
            {
                SyntaxTriviaHelper.GetEndOfLine(),
                SyntaxTriviaHelper.GetTab(),
                SyntaxTriviaHelper.GetRegion("Private use"),
                SyntaxTriviaHelper.GetEndOfLine(),
                SyntaxTriviaHelper.GetEndOfLine()
            };
            var newFirstDeclaration = firstDeclaration.WithLeadingTrivia(firstPrivateMethodLeadingTrivias);
            var newMembers = classDeclaration.Members.Replace(firstDeclaration, newFirstDeclaration);

            var closeBraceLeadingTrivia = new List<SyntaxTrivia>
            {
                SyntaxTriviaHelper.GetEndOfLine(),
                SyntaxTriviaHelper.GetTab(),
                SyntaxTriviaHelper.GetEndRegion(),
                SyntaxTriviaHelper.GetEndOfLine()
            };
            var newCloseBrace = classDeclaration.CloseBraceToken.WithLeadingTrivia(closeBraceLeadingTrivia);
            documentEditor.ReplaceNode(
                classDeclaration,
                classDeclaration.WithMembers(newMembers).WithCloseBraceToken(newCloseBrace)
            );
        }

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}