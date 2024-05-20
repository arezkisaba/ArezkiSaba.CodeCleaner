using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class DeleteEmptyLinesAroundBraces
{
    public async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        bool isUpdated;
        DocumentEditor documentEditor;
        List<SyntaxToken> tokens = [];

        // After open brace

        do
        {
            isUpdated = false;
            documentEditor = await DocumentEditor.CreateAsync(document);
            tokens = documentEditor.OriginalRoot.DescendantTokens().ToList();

            for (var i = tokens.Count - 1; i >= 0; i--)
            {
                var token = tokens[i];
                if (token.IsKind(SyntaxKind.OpenBraceToken))
                {
                    var targetToken = tokens[i + 1];
                    IEnumerable<SyntaxTrivia> leadingTrivias = null;

                    if (targetToken.LeadingTrivia.All(obj => obj.IsKind(SyntaxKind.EndOfLineTrivia) || obj.IsKind(SyntaxKind.WhitespaceTrivia)))
                    {
                        leadingTrivias = targetToken.LeadingTrivia.Where(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));
                    }
                    else
                    {
                        leadingTrivias = targetToken.LeadingTrivia.AsEnumerable();
                    }

                    var targetTokenUpdated = targetToken.WithLeadingTrivia(leadingTrivias);
                    var node = targetToken.Parent;
                    var nodeUpdated = node.ReplaceToken(targetToken, targetTokenUpdated);

                    if (!targetToken.IsEqualTo(targetTokenUpdated))
                    {
                        documentEditor.ReplaceNode(node, nodeUpdated);
                        document = documentEditor.GetChangedDocument();
                        isUpdated = true;
                        break;
                    }
                }
            }
        } while (isUpdated);

        // Before close brace

        do
        {
            isUpdated = false;
            documentEditor = await DocumentEditor.CreateAsync(document);
            tokens = documentEditor.OriginalRoot.DescendantTokens().ToList();

            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token.IsKind(SyntaxKind.CloseBraceToken))
                {
                    var targetToken = token;
                    IEnumerable<SyntaxTrivia> leadingTrivias = null;

                    if (targetToken.LeadingTrivia.All(obj => obj.IsKind(SyntaxKind.EndOfLineTrivia) || obj.IsKind(SyntaxKind.WhitespaceTrivia)))
                    {
                        leadingTrivias = targetToken.LeadingTrivia.Where(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));
                    }
                    else
                    {
                        leadingTrivias = targetToken.LeadingTrivia.AsEnumerable();
                    }

                    var targetTokenUpdated = targetToken.WithLeadingTrivia(leadingTrivias);
                    var node = targetToken.Parent;
                    var nodeUpdated = node.ReplaceToken(targetToken, targetTokenUpdated);

                    if (!targetToken.IsEqualTo(targetTokenUpdated))
                    {
                        documentEditor.ReplaceNode(node, nodeUpdated);
                        document = documentEditor.GetChangedDocument();
                        isUpdated = true;
                        break;
                    }
                }
            }
        } while (isUpdated);

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}