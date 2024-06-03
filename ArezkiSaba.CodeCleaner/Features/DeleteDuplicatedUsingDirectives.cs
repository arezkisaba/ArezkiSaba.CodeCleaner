using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class DeleteDuplicatedUsingDirectives : RefactorOperationBase
{
    public override string Name => nameof(DeleteDuplicatedUsingDirectives);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var usingDirectives = documentEditor.OriginalRoot.ChildNodes().OfType<UsingDirectiveSyntax>().ToList();
        var usingDirectivesToRemove = new List<UsingDirectiveSyntax>();

        for (var i = 0; i < usingDirectives.Count; i++)
        {
            var hasSameUsingDirectives = usingDirectives.Skip(i + 1).Any(obj => obj.Name.ToString() == usingDirectives[i].Name.ToString());
            if (hasSameUsingDirectives)
            {
                usingDirectivesToRemove.Add(usingDirectives[i]);
            }
        }

        foreach (var usingDirectiveToRemove in usingDirectivesToRemove)
        {
            documentEditor.RemoveNode(usingDirectiveToRemove);
        }

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}