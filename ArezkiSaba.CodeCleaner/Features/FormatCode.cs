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
                            var childToken = childNode.FirstChildToken();
                            var newChildNode = childNode.ReplaceToken(childToken, childToken.WithLeadingTrivia(GetLeadingTriviasBasedOn(parentNode, indentCount: 1)));

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
                            var needLineBreak = true;
                            var newParameterList = parameterList.ReplaceNodes(parameterList.Parameters, (parameter, __) =>
                            {
                                if (needLineBreak)
                                {
                                    parameter = parameter.WithLeadingTrivia(GetLeadingTriviasBasedOn(parentNode, indentCount: 1)).WithoutTrailingTrivia();
                                }

                                return parameter;
                            });

                            var i = 0;
                            newParameterList = newParameterList.WithOpenParenToken(newParameterList.OpenParenToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine()));
                            newParameterList = newParameterList.ReplaceTokens(newParameterList.Parameters.GetSeparators(), (separator, __) =>
                            {
                                return separator.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine());
                            });
                            newParameterList = newParameterList.WithCloseParenToken(newParameterList.CloseParenToken.WithoutLeadingTrivias());

                            var newBaseMethodDeclaration = baseMethodDeclaration.WithParameterList(newParameterList);
                            if (!baseMethodDeclaration.IsEqualTo(newBaseMethodDeclaration))
                            {
                                documentEditor.ReplaceNode(baseMethodDeclaration, newBaseMethodDeclaration);
                                return (documentEditor.GetChangedDocument(), true);
                            }
                        }

                        if (childNode is ConstructorInitializerSyntax constructorInitializer)
                        {
                            var newParametersList = baseMethodDeclaration.ParameterList.WithCloseParenToken(
                                baseMethodDeclaration.ParameterList.CloseParenToken.WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine())
                            );
                            var newBaseMethodDeclaration = baseMethodDeclaration.WithParameterList(newParametersList);
                            if (!baseMethodDeclaration.IsEqualTo(newBaseMethodDeclaration))
                            {
                                documentEditor.ReplaceNode(baseMethodDeclaration, newBaseMethodDeclaration);
                                return (documentEditor.GetChangedDocument(), true);
                            }

                            var childToken = childNode.FirstChildToken(recursive: true);
                            var newChildNode = childNode.ReplaceToken(childToken, childToken.WithLeadingTrivia(GetLeadingTriviasBasedOn(parentNode, indentCount: 1)));
                            if (!childNode.IsEqualTo(newChildNode))
                            {
                                documentEditor.ReplaceNode(childNode, newChildNode);
                                return (documentEditor.GetChangedDocument(), true);
                            }
                        }

                        if (childNode is BlockSyntax)
                        {
                            var childTokens = childNode.ChildTokens();
                            var newChildNode = childNode.ReplaceTokens(childTokens, (childToken, __) =>
                            {
                                if (childToken.IsKind(SyntaxKind.OpenBraceToken) || childToken.IsKind(SyntaxKind.CloseBraceToken))
                                {
                                    return childToken.WithLeadingTrivia(GetLeadingTriviasBasedOn(parentNode));
                                }

                                return childToken;
                            });

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
                            var newChildNode = childNode.ReplaceToken(childToken, childToken.WithLeadingTrivia(GetLeadingTriviasBasedOn(parentNode, indentCount: 1)));

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

    public static IList<SyntaxTrivia> GetLeadingTriviasBasedOn(
        SyntaxNode nodeBase,
        int indentCount = 0,
        bool addLeadingEndOfLine = false)
    {
        var leadingTrivias = new List<SyntaxTrivia>();
        if (addLeadingEndOfLine)
        {
            leadingTrivias.Add(SyntaxTriviaHelper.GetEndOfLine());
        }

        leadingTrivias.AddRange(nodeBase.GetIndentationTrivias());

        for (var j = 0; j < indentCount; j++)
        {
            leadingTrivias.Add(SyntaxTriviaHelper.GetTab());
        }

        return leadingTrivias;
    }

    public static IList<SyntaxTrivia> GetLeadingTrivia(
        int count)
    {
        var leadingTrivias = new List<SyntaxTrivia>();

        for (var i = 0; i < count; i++)
        {
            leadingTrivias.Add(SyntaxTriviaHelper.GetTab());
        }

        return leadingTrivias;
    }

    #endregion
}