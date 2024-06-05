using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class FormatCode : RefactorOperationBase
{
    public override string Name => nameof(FormatCode);

    public override async Task<RefactorOperationResult> StartAsync(
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

    private (Document Document, bool Updated) FormatCodeInternal(
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
                (Document Document, bool Updated) result;
                if (child.IsNode)
                {
                    var childNode = child.AsNode();
                    var newIndentLevel = indentLevel;

                    result = HandleParentNodeAsTypeDeclarationSyntax(documentEditor, parentNode, childNode);
                    if (result.Updated)
                    {
                        return result;
                    }

                    result = HandleParentNodeAsBaseMethodDeclarationSyntax(documentEditor, parentNode, childNode);
                    if (result.Updated)
                    {
                        return result;
                    }

                    ////result = HandleParentNodeAsAccessorDeclarationSyntax(documentEditor, parentNode, childNode);
                    ////if (result.Updated)
                    ////{
                    ////    return result;
                    ////}

                    ////result = HandleParentNodeAsBlockSyntax(documentEditor, parentNode, childNode);
                    ////if (result.Updated)
                    ////{
                    ////    return result;
                    ////}
                    ////result = HandleChildNodeAsIfStatementSyntax(documentEditor, parentNode, childNode);
                    ////if (result.Updated)
                    ////{
                    ////    return result;
                    ////}
                    ////result = HandleChildNodeAsForStatementSyntax(documentEditor, parentNode, childNode);
                    ////if (result.Updated)
                    ////{
                    ////    return result;
                    ////}
                    ////result = HandleChildNodeAsWhileStatementSyntax(documentEditor, parentNode, childNode);
                    ////if (result.Updated)
                    ////{
                    ////    return result;
                    ////}
                    ////result = HandleChildNodeAsBlockSyntax(documentEditor, parentNode, childNode);
                    ////if (result.Updated)
                    ////{
                    ////    return result;
                    ////}
                    // recursive method
                    result = FormatCodeInternal(documentEditor, child.AsNode(), newIndentLevel);
                    if (result.Updated)
                    {
                        return result;
                    }
                }
            }
        }

        return (documentEditor.GetChangedDocument(), false);
    }

    private (Document Document, bool Updated) HandleParentNodeAsTypeDeclarationSyntax(
        DocumentEditor documentEditor,
        SyntaxNode parentNode,
        SyntaxNode childNode)
    {
        if (parentNode is TypeDeclarationSyntax)
        {
            if (childNode is BaseMethodDeclarationSyntax)
            {
                var newChildNode = childNode.WithIndentationTrivia(parentNode, keepOtherTrivias: true);
                if (!childNode.IsEqualTo(newChildNode))
                {
                    documentEditor.ReplaceNode(childNode, newChildNode);
                    return (documentEditor.GetChangedDocument(), true);
                }
            }
        }

        return (documentEditor.GetChangedDocument(), false);
    }

    private (Document Document, bool Updated) HandleParentNodeAsBaseMethodDeclarationSyntax(
        DocumentEditor documentEditor,
        SyntaxNode parentNode,
        SyntaxNode childNode)
    {
        if (parentNode is BaseMethodDeclarationSyntax baseMethodDeclaration)
        {
            if (childNode is ParameterListSyntax parameterList)
            {
                var newBaseMethodDeclaration = baseMethodDeclaration.WithParameterList(parameterList.Format(parentNode));
                if (!baseMethodDeclaration.IsEqualTo(newBaseMethodDeclaration))
                {
                    documentEditor.ReplaceNode(baseMethodDeclaration, newBaseMethodDeclaration);
                    return (documentEditor.GetChangedDocument(), true);
                }
            }

            if (childNode is ConstructorInitializerSyntax constructorInitializer &&
                baseMethodDeclaration is ConstructorDeclarationSyntax constructorDeclaration)
            {
                var newConstructorDeclaration = constructorDeclaration.Format(constructorInitializer, constructorDeclaration);
                if (!constructorDeclaration.IsEqualTo(newConstructorDeclaration))
                {
                    documentEditor.ReplaceNode(constructorDeclaration, newConstructorDeclaration);
                    return (documentEditor.GetChangedDocument(), true);
                }
            }
        }

        return (documentEditor.GetChangedDocument(), false);
    }

    private (Document Document, bool Updated) HandleParentNodeAsAccessorDeclarationSyntax(
        DocumentEditor documentEditor,
        SyntaxNode parentNode,
        SyntaxNode childNode)
    {
        if (parentNode is AccessorDeclarationSyntax accessorDeclaration)
        {
            var newAccessorDeclaration = accessorDeclaration.Format();
            if (!accessorDeclaration.IsEqualTo(newAccessorDeclaration))
            {
                documentEditor.ReplaceNode(accessorDeclaration, newAccessorDeclaration);
                return (documentEditor.GetChangedDocument(), true);
            }
        }

        return (documentEditor.GetChangedDocument(), false);
    }

    private (Document Document, bool Updated) HandleParentNodeAsBlockSyntax(
        DocumentEditor documentEditor,
        SyntaxNode parentNode,
        SyntaxNode childNode)
    {
        if (parentNode is BlockSyntax)
        {
            if (childNode is StatementSyntax statementSyntax)
            {
                var newChildNode = childNode.WithIndentationTrivia(parentNode, keepOtherTrivias: true);
                if (!childNode.IsEqualTo(newChildNode))
                {
                    documentEditor.ReplaceNode(childNode, newChildNode);
                    return (documentEditor.GetChangedDocument(), true);
                }
            }
        }

        return (documentEditor.GetChangedDocument(), false);
    }

    private (Document Document, bool Updated) HandleChildNodeAsIfStatementSyntax(
        DocumentEditor documentEditor,
        SyntaxNode parentNode,
        SyntaxNode childNode)
    {
        if (childNode is IfStatementSyntax ifStatement)
        {
            var newIfStatement = ifStatement.AddBracesBasedOnParent(parentNode);
            if (!ifStatement.IsEqualTo(newIfStatement))
            {
                documentEditor.ReplaceNode(ifStatement, newIfStatement);
                return (documentEditor.GetChangedDocument(), true);
            }
        }

        return (documentEditor.GetChangedDocument(), false);
    }

    private (Document Document, bool Updated) HandleChildNodeAsForStatementSyntax(
        DocumentEditor documentEditor,
        SyntaxNode parentNode,
        SyntaxNode childNode)
    {
        if (childNode is ForStatementSyntax forStatement)
        {
            var newForStatement = forStatement.AddBracesBasedOnParent(parentNode);
            if (!forStatement.IsEqualTo(newForStatement))
            {
                documentEditor.ReplaceNode(forStatement, newForStatement);
                return (documentEditor.GetChangedDocument(), true);
            }
        }

        return (documentEditor.GetChangedDocument(), false);
    }

    private (Document Document, bool Updated) HandleChildNodeAsWhileStatementSyntax(
        DocumentEditor documentEditor,
        SyntaxNode parentNode,
        SyntaxNode childNode)
    {
        if (childNode is WhileStatementSyntax whileStatement)
        {
            var newWhileStatement = whileStatement.AddBracesBasedOnParent(parentNode);
            if (!whileStatement.IsEqualTo(newWhileStatement))
            {
                documentEditor.ReplaceNode(whileStatement, newWhileStatement);
                return (documentEditor.GetChangedDocument(), true);
            }
        }

        return (documentEditor.GetChangedDocument(), false);
    }

    private (Document Document, bool Updated) HandleChildNodeAsBlockSyntax(
        DocumentEditor documentEditor,
        SyntaxNode parentNode,
        SyntaxNode childNode)
    {
        if (childNode is BlockSyntax block)
        {
            var newBlock = block.AddTabLeadingTriviasOnBracesBasedOnParent(parentNode);
            if (!block.IsEqualTo(newBlock))
            {
                documentEditor.ReplaceNode(block, newBlock);
                return (documentEditor.GetChangedDocument(), true);
            }
        }

        return (documentEditor.GetChangedDocument(), false);
    }

    private static int GetImbricationLevel(
        ExpressionSyntax expression)
    {
        var imbricationLevel = 0;
        var ancestors = expression.Ancestors().ToList();
        foreach (var ancestor in ancestors)
        {
            if (ancestor.IsImbricationExpression())
            {
                imbricationLevel++;
            }
            else if (ancestor is StatementSyntax)
            {
                break;
            }
        }

        return imbricationLevel;
    }

    #endregion
}