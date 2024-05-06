using ArezkiSaba.CodeCleaner.Rewriters;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class DocumentExtensions
{
    public static async Task<Document> StartTypeInferenceRewriterAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        return document.WithSyntaxRoot(new TypeInferenceRewriter(semanticModel).Visit(root));
    }

    public static async Task<Document> StartReadonlyModifierFieldRewriterAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        return document.WithSyntaxRoot(
            new ReadonlyModifierFieldRewriter(
                document.Project.Solution,
                semanticModel
            ).Visit(root)
        );
    }

    public static async Task<Document> StartSealedModifierClassRewriterAsync(
        this Document document)
    {
        var allTypeDeclarations = await GetAllTypeDeclarations(document);
        var root = await document.GetSyntaxRootAsync();
        var semanticModel = await document.GetSemanticModelAsync();
        return document.WithSyntaxRoot(
            new SealedModifierClassRewriter(
                document.Project.Solution,
                semanticModel,
                allTypeDeclarations
            ).Visit(root)
        );
    }

    public static async Task<Document> StartUsingDirectiveSorterAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        var compilationUnit = root as CompilationUnitSyntax;
        var sortedUsingDirectives = SyntaxFactory.List(compilationUnit.Usings
            .OrderBy(x => GetUsingDirectiveRank(x))
            .ThenBy(x => x.Name.ToString().GetAlphaNumerics())
        );
        compilationUnit = compilationUnit.WithUsings(sortedUsingDirectives);
        document = document.WithSyntaxRoot(compilationUnit);
        return document;
    }

    public static async Task<Document> StartDuplicatedUsingDirectiveRemoverAsync(
        this Document document)
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

        return documentEditor.GetChangedDocument();
    }

    public static async Task<Document> StartEmptyLinesBracesRemoverAsync(
        this Document document)
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
                    if (targetToken.HasLeadingTrivia)
                    {
                        var targetTokenUpdated = targetToken.WithLeadingTrivia();
                        var node = targetToken.Parent;
                        var nodeUpdated = node.ReplaceToken(targetToken, targetTokenUpdated);

                        if (targetToken.HasLeadingTrivia != targetTokenUpdated.HasLeadingTrivia)
                        {
                            documentEditor.ReplaceNode(node, nodeUpdated);
                            document = documentEditor.GetChangedDocument();
                            isUpdated = true;
                            break;
                        }
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
                    var targetTokenUpdated = targetToken.WithLeadingTrivia();
                    var node = targetToken.Parent;
                    var nodeUpdated = node.ReplaceToken(targetToken, targetTokenUpdated);

                    if (targetToken.HasLeadingTrivia != targetTokenUpdated.HasLeadingTrivia)
                    {
                        documentEditor.ReplaceNode(node, nodeUpdated);
                        document = documentEditor.GetChangedDocument();
                        isUpdated = true;
                        break;
                    }
                }
            }
        } while (isUpdated);

        return documentEditor.GetChangedDocument();
    }

    public static async Task<Document> StartDuplicatedEmptyLinesRemoverAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        return document.WithSyntaxRoot(new DuplicatedEmptyLinesRemover().Visit(root));
    }

    public static async Task<Document> ReorderClassMembersAsync(
        this Document document)
    {
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var root = documentEditor.GetDocumentEditorRoot();

        // Take interfaces, classes, structs
        var typeDeclarations = root.ChildNodes().OfType<TypeDeclarationSyntax>()
            .Reverse()
            .ToList();
        foreach (var typeDeclaration in typeDeclarations)
        {
            var newTypeDeclaration = await GetSortedTypeDeclaration(documentEditor, typeDeclaration);
            documentEditor.ReplaceNode(typeDeclaration, newTypeDeclaration);
        }

        return documentEditor.GetChangedDocument();
    }

    public static async Task<Document> StartRegionInserterAsync(
        this Document document)
    {
        if (document.SkipProgramEntryPoint())
        {
            return document;
        }

        var documentEditor = await DocumentEditor.CreateAsync(document);
        var declarations = documentEditor.OriginalRoot.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(obj => obj.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword)))
            .ToList();

        if (!declarations.Any())
        {
            return document;
        }

        var firstDeclaration = declarations.First();
        var lastDeclaration = declarations.Last();

        var leadingTrivia = firstDeclaration.GetLeadingTrivia()
            .Add(SyntaxFactory.Trivia(SyntaxFactory.RegionDirectiveTrivia(true)
                .WithTrailingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxFactory.Whitespace(" "),
                        SyntaxFactory.PreprocessingMessage("\"Private use\"")
                    )
                )
            ))
            .Add(SyntaxTriviaHelper.GetEndOfLine())
            .Add(SyntaxTriviaHelper.GetEndOfLine())
            .Add(SyntaxTriviaHelper.GetTab());
        var trailingTrivia = lastDeclaration.GetTrailingTrivia()
            .Add(SyntaxTriviaHelper.GetEndOfLine())
            .Add(SyntaxTriviaHelper.GetTab())
            .Add(SyntaxFactory.Trivia(SyntaxFactory.EndRegionDirectiveTrivia(true)))
            .Add(SyntaxTriviaHelper.GetEndOfLine());

        if (ReferenceEquals(firstDeclaration, lastDeclaration))
        {
            documentEditor.ReplaceNode(firstDeclaration, firstDeclaration
                .WithLeadingTrivia(leadingTrivia)
                .WithTrailingTrivia(trailingTrivia)
            );
        }
        else
        {
            documentEditor.ReplaceNode(
                firstDeclaration,
                firstDeclaration.WithLeadingTrivia(leadingTrivia)
            );
            documentEditor.ReplaceNode(
                lastDeclaration,
                lastDeclaration.WithTrailingTrivia(trailingTrivia)
            );
        }

        return documentEditor.GetChangedDocument();
    }

    public static async Task<Document> StartMethodDeclarationParameterLineBreakerAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        return document.WithSyntaxRoot(new MethodDeclarationParameterLineBreaker().Visit(root));
    }

    public static async Task<Document> StartInvocationExpressionArgumentLineBreakerAsync(
        this Document document)
    {
        var root = await document.GetSyntaxRootAsync();
        return document.WithSyntaxRoot(new InvocationExpressionArgumentLineBreaker().Visit(root));
    }

    public static bool IsAutoGenerated(
        this Document document)
    {
        var fileName = Path.GetFileNameWithoutExtension(document.FilePath.AsSpan());
        var isFilePathAutoGenerated =
            document.FilePath?.Contains(".nuget", StringComparison.OrdinalIgnoreCase) ?? false;
        var isFileNameAutoGenerated =
            fileName.Contains(".AssemblyAttributes", StringComparison.OrdinalIgnoreCase) ||
            fileName.Contains(".AssemblyInfo", StringComparison.OrdinalIgnoreCase) ||
            fileName.StartsWith("TemporaryGeneratedFile_", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".designer", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".generated", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".g", StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(".gi", StringComparison.OrdinalIgnoreCase);
        return isFilePathAutoGenerated || isFileNameAutoGenerated;
    }

    public static bool SkipProgramEntryPoint(
        this Document document)
    {
        return document.Name == "Program.cs";
    }

    public static SyntaxNode GetDocumentEditorRoot(
        this DocumentEditor documentEditor)
    {
        var root = documentEditor.OriginalRoot.ChildNodes().FirstOrDefault(
            obj => obj.IsKind(SyntaxKind.NamespaceDeclaration) || obj.IsKind(SyntaxKind.FileScopedNamespaceDeclaration)
        );
        root ??= documentEditor.OriginalRoot;
        return root;
    }

    #region Private use

    private static int GetUsingDirectiveRank(
        UsingDirectiveSyntax usingDirective)
    {
        if (usingDirective.Alias == null)
        {
            if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
            {
                return 7;
            }
            else if (usingDirective.Name.ToString().StartsWith("System"))
            {
                return 1;
            }
            else if (usingDirective.Name.ToString().StartsWith("Microsoft"))
            {
                return 2;
            }
            else if (usingDirective.Name.ToString().StartsWith("Windows"))
            {
                return 3;
            }
            else if (usingDirective.Name.ToString().StartsWith("Prevoir.Toolkit"))
            {
                return 5;
            }
            else if (usingDirective.Name.ToString().StartsWith("Prevoir."))
            {
                return 6;
            }
            else
            {
                return 4;
            }
        }
        else
        {
            return 8;
        }
    }

    private static async Task<IList<TypeDeclarationSyntax>> GetAllTypeDeclarations(
        Document document)
    {
        var typeDeclarations = new List<TypeDeclarationSyntax>();

        foreach (var project in document.Project.Solution.Projects)
        {
            foreach (var currentDocument in project.Documents)
            {
                var syntaxTree = await currentDocument.GetSyntaxTreeAsync();
                if (syntaxTree != null && syntaxTree.Options.Kind == SourceCodeKind.Regular)
                {
                    var currentRoot = await syntaxTree.GetRootAsync();
                    var typeDeclarationsInDocument = currentRoot.DescendantNodes().OfType<TypeDeclarationSyntax>();
                    typeDeclarations.AddRange(typeDeclarationsInDocument);
                }
            }
        }

        return typeDeclarations;
    }

    private static async Task<TypeDeclarationSyntax> GetSortedTypeDeclaration(
        DocumentEditor documentEditor,
        TypeDeclarationSyntax typeDeclarationRoot)
    {
        var declarationsToExtract = new List<SyntaxKind>()
        {
            SyntaxKind.FieldDeclaration,
            SyntaxKind.EventFieldDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.OperatorDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.InterfaceDeclaration,
        };
        var indentationTrivia = typeDeclarationRoot.DescendantTrivia().First(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia));
        var memberDeclarationsReversed = typeDeclarationRoot.ChildNodes().OfType<MemberDeclarationSyntax>().Reverse().ToList();

        var memberDeclarationsToAdd = new List<MemberDeclarationSyntax>();
        foreach (var memberDeclaration in memberDeclarationsReversed)
        {
            var newMemberDeclaration = memberDeclaration;

            if (!declarationsToExtract.Any(newMemberDeclaration.IsKind))
            {
                continue;
            }

            if (newMemberDeclaration is TypeDeclarationSyntax typeDeclaration)
            {
                var newTypeDeclaration = await GetSortedTypeDeclaration(documentEditor, typeDeclaration);
                documentEditor.ReplaceNode(typeDeclaration, newTypeDeclaration);
                newMemberDeclaration = newTypeDeclaration;
            }

            memberDeclarationsToAdd.Add(newMemberDeclaration);
        }

        var orderedMemberDeclarations = new List<MemberDeclarationSyntax>();
        foreach (var declarationToExtract in declarationsToExtract)
        {
            orderedMemberDeclarations.AddRange(GetMemberDeclarations(memberDeclarationsToAdd, declarationToExtract, indentationTrivia));
        }

        return typeDeclarationRoot.WithMembers(new SyntaxList<MemberDeclarationSyntax>(orderedMemberDeclarations));
    }

    private static List<MemberDeclarationSyntax> GetMemberDeclarations(
        List<MemberDeclarationSyntax> memberDeclarations,
        SyntaxKind syntaxKind,
        SyntaxTrivia indentationTrivia)
    {
        var sortedemberDeclarations = memberDeclarations
            .Where(obj => obj.IsKind(syntaxKind))
            .OrderBy(obj => GetMemberDeclarationModifierRank(obj, syntaxKind))
            .ThenBy(obj => obj.GetName())
            .Select((obj, i) =>
            {
                var leadingTrivias = new List<SyntaxTrivia>();
                if (i == 0 || (syntaxKind != SyntaxKind.FieldDeclaration && syntaxKind != SyntaxKind.EventFieldDeclaration))
                {
                    leadingTrivias.Add(SyntaxTriviaHelper.GetEndOfLine());
                }

                leadingTrivias.Add(indentationTrivia);
                leadingTrivias.Add(SyntaxTriviaHelper.GetTab());

                return obj
                    .RemoveAllTriviasFromParametersAndArguments()
                    .WithLeadingTrivia(
                        SyntaxFactory.TriviaList(leadingTrivias)
                    )
                    .WithTrailingTrivia(
                        SyntaxFactory.TriviaList(
                            SyntaxTriviaHelper.GetEndOfLine()
                        )
                    );
            });
        return sortedemberDeclarations.ToList();
    }

    private static int GetMemberDeclarationModifierRank(
        MemberDeclarationSyntax memberDeclaration,
        SyntaxKind syntaxKind)
    {
        var modifiers = memberDeclaration.Modifiers;

        if (syntaxKind == SyntaxKind.FieldDeclaration)
        {
            if (modifiers.Any(SyntaxKind.ConstKeyword))
            {
                return 1;
            }
            else if (modifiers.Any(SyntaxKind.StaticKeyword))
            {
                return 2;
            }
            else if (modifiers.Any(SyntaxKind.ReadOnlyKeyword))
            {
                return 3;
            }
            else
            {
                return 4;
            }
        }

        if (syntaxKind == SyntaxKind.ConstructorDeclaration)
        {
            var constructorDeclaration = (ConstructorDeclarationSyntax)memberDeclaration;
            return constructorDeclaration.ParameterList.Parameters.Count;
        }

        if (modifiers.Any(SyntaxKind.PublicKeyword) &&
            modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return 1;
        }
        else if (modifiers.Any(SyntaxKind.PublicKeyword))
        {
            return 2;
        }
        else if (modifiers.Any(SyntaxKind.ProtectedKeyword))
        {
            return 3;
        }
        else if (modifiers.Any(SyntaxKind.InternalKeyword))
        {
            return 4;
        }
        else if (modifiers.Any(SyntaxKind.PrivateKeyword))
        {
            return 5;
        }
        else if (modifiers.Any(SyntaxKind.StaticKeyword))
        {
            return 6;
        }

        return 7;
    }

    #endregion
}
