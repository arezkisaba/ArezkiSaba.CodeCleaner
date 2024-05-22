using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class FormatCode
{
    public async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        bool isUpdated;
        DocumentEditor documentEditor;

        do
        {
            isUpdated = false;
            documentEditor = await DocumentEditor.CreateAsync(document);
            (document, isUpdated) = FormatCodeInternal(documentEditor, documentEditor.OriginalRoot);
        } while (isUpdated);

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    #region Private use

    private static (Document Document, bool Updated) FormatCodeInternal(
        DocumentEditor documentEditor,
        SyntaxNode parentNode,
        int indentLevel = 0)
    {
        var children = parentNode.ChildNodesAndTokens().ToList();

        if (children.Count() == 0)
        {
            return (documentEditor.GetChangedDocument(), false);
        }
        else
        {
            foreach (var child in children)
            {
                if (child.IsNode)
                {
                    var childNode = child.AsNode();
                    var newIndentLevel = indentLevel;

                    if (parentNode is TypeDeclarationSyntax)
                    {
                        if (childNode is BaseMethodDeclarationSyntax)
                        {
                            var newChildNode = childNode.WriteIndentationTrivia(parentNode);
                            if (!childNode.IsEqualTo(newChildNode))
                            {
                                documentEditor.ReplaceNode(childNode, newChildNode);
                                return (documentEditor.GetChangedDocument(), true);
                            }
                        }
                    }

                    if (parentNode is BaseMethodDeclarationSyntax baseMethodDeclaration)
                    {
                        if (childNode is ParameterListSyntax parameterList)
                        {
                            var newBaseMethodDeclaration = baseMethodDeclaration.FormatParameters(parameterList, parentNode);
                            if (!baseMethodDeclaration.IsEqualTo(newBaseMethodDeclaration))
                            {
                                documentEditor.ReplaceNode(baseMethodDeclaration, newBaseMethodDeclaration);
                                return (documentEditor.GetChangedDocument(), true);
                            }
                        }

                        if (childNode is ConstructorInitializerSyntax constructorInitializer &&
                            baseMethodDeclaration is ConstructorDeclarationSyntax constructorDeclaration)
                        {
                            var newConstructorDeclaration = constructorDeclaration.FormatInitializer(constructorInitializer, parentNode);
                            if (!constructorDeclaration.IsEqualTo(newConstructorDeclaration))
                            {
                                documentEditor.ReplaceNode(constructorDeclaration, newConstructorDeclaration);
                                return (documentEditor.GetChangedDocument(), true);
                            }
                        }

                        if (childNode is BlockSyntax block)
                        {
                            var newChildNode = block.AddTabLeadingTriviasOnBracesBasedOnParent(parentNode);
                            if (!childNode.IsEqualTo(newChildNode))
                            {
                                documentEditor.ReplaceNode(childNode, newChildNode);
                                return (documentEditor.GetChangedDocument(), true);
                            }
                        }
                    }

                    if (parentNode is BlockSyntax)
                    {
                        if (childNode is StatementSyntax statementSyntax)
                        {
                            var childToken = childNode.FirstChildToken(recursive: true);
                            var newChildNode = childNode.ReplaceToken(
                                childToken,
                                childToken.WithLeadingTrivia(
                                    SyntaxTriviaHelper.GetLeadingTriviasBasedOn(parentNode, indentCount: 1)
                                )
                            );

                            if (!childNode.IsEqualTo(newChildNode))
                            {
                                documentEditor.ReplaceNode(childNode, newChildNode);
                                return (documentEditor.GetChangedDocument(), true);
                            }
                        }
                    }

                    // recursive method
                    var result = FormatCodeInternal(documentEditor, child.AsNode(), newIndentLevel);
                    if (result.Updated)
                    {
                        return result;
                    }
                }
            }
        }

        return (documentEditor.GetChangedDocument(), false);
    }

    #endregion
}