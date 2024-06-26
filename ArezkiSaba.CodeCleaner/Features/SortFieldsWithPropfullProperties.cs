﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class SortFieldsWithPropfullProperties : RefactorOperationBase
{
    public override string Name => nameof(SortFieldsWithPropfullProperties);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var root = documentEditor.GetDocumentEditorRoot();
        var semanticModel = await document.GetSemanticModelAsync();

        var typeDeclarations = root.DescendantNodes().OfType<TypeDeclarationSyntax>()
            .Reverse()
            .ToList();
        foreach (var typeDeclaration in typeDeclarations)
        {
            var fieldDeclarations = typeDeclaration.ChildNodes().OfType<FieldDeclarationSyntax>()
                .Reverse()
                .ToList();
            foreach (var fieldDeclaration in fieldDeclarations)
            {
                var variableDeclarator = fieldDeclaration.DescendantNodes().OfType<VariableDeclaratorSyntax>().FirstOrDefault();
                var symbol = semanticModel.GetDeclaredSymbol(variableDeclarator);
                var references = await SymbolFinder.FindReferencesAsync(symbol, solution);
                foreach (var reference in references)
                {
                    foreach (var location in reference.Locations)
                    {
                        var documentRoot = await location.Document.GetSyntaxRootAsync();
                        var referencedNode = documentRoot.FindNode(location.Location.SourceSpan);
                        var propertyDeclaration = referencedNode.Ancestors()
                            .OfType<PropertyDeclarationSyntax>()
                            .FirstOrDefault();

                        var fieldName = fieldDeclaration.GetName();
                        var propertyName = propertyDeclaration.GetName();

                        if (fieldName.ToPascalCase() != propertyName)
                        {
                            continue;
                        }

                        var isReferencedFromProperty = propertyDeclaration != null;
                        if (isReferencedFromProperty)
                        {
                            var newFieldDeclaration = fieldDeclaration
                                .WithIndentationTrivia(propertyDeclaration, indentCount: 0, mustAddLineBreakBefore: true)
                                .WithEndOfLineTrivia();
                            var newPropertyDeclaration = propertyDeclaration
                                .WithIndentationTrivia(propertyDeclaration, indentCount: 0);

                            documentEditor.InsertBefore(propertyDeclaration, newFieldDeclaration);
                            documentEditor.ReplaceNode(propertyDeclaration, newPropertyDeclaration);
                            documentEditor.RemoveNode(fieldDeclaration);
                            break;
                        }
                    }
                }
            }
        }

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }
}