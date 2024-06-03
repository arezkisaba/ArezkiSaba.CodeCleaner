using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using ArezkiSaba.CodeCleaner.Extensions;
using ArezkiSaba.CodeCleaner.Models;

namespace ArezkiSaba.CodeCleaner.Features;

public sealed class SortClassMembers : RefactorOperationBase
{
    public override string Name => nameof(SortClassMembers);

    public override async Task<RefactorOperationResult> StartAsync(
        Document document,
        Solution solution)
    {
        var documentEditor = await DocumentEditor.CreateAsync(document);
        var root = documentEditor.GetDocumentEditorRoot();

        // Take interfaces, classes, structs
        var typeDeclarations = root.ChildNodes().OfType<TypeDeclarationSyntax>()
            .Reverse()
            .ToList();
        foreach (var typeDeclaration in typeDeclarations)
        {
            var newTypeDeclaration = await GetSortedTypeDeclarationAsync(documentEditor, typeDeclaration);
            documentEditor.ReplaceNode(typeDeclaration, newTypeDeclaration);
        }

        document = documentEditor.GetChangedDocument();
        return new RefactorOperationResult(
            document,
            document.Project,
            document.Project.Solution
        );
    }

    #region Private use

    private async Task<TypeDeclarationSyntax> GetSortedTypeDeclarationAsync(
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
                var newTypeDeclaration = await GetSortedTypeDeclarationAsync(documentEditor, typeDeclaration);
                documentEditor.ReplaceNode(typeDeclaration, newTypeDeclaration);
                newMemberDeclaration = newTypeDeclaration;
            }

            memberDeclarationsToAdd.Add(newMemberDeclaration);
        }

        var orderedMemberDeclarations = new List<MemberDeclarationSyntax>();
        foreach (var declarationToExtract in declarationsToExtract)
        {
            orderedMemberDeclarations.AddRange(GetMemberDeclarations(memberDeclarationsToAdd, declarationToExtract, typeDeclarationRoot));
        }

        return typeDeclarationRoot
            .WithMembers(new SyntaxList<MemberDeclarationSyntax>(orderedMemberDeclarations));
    }

    private List<MemberDeclarationSyntax> GetMemberDeclarations(
        List<MemberDeclarationSyntax> memberDeclarations,
        SyntaxKind syntaxKind,
        SyntaxNode parentNode)
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

                var baseLeadingTrivia = parentNode.FindFirstLeadingTrivia();
                var commentsTrivia = obj.GetLeadingTrivia().Where(obj => obj.IsCommentTrivia());
                if (commentsTrivia.Any())
                {
                    foreach (var commentTrivia in commentsTrivia)
                    {
                        if (baseLeadingTrivia != null)
                        {
                            leadingTrivias.Add(baseLeadingTrivia.Value);
                        }

                        leadingTrivias.Add(SyntaxTriviaHelper.GetTab());
                        leadingTrivias.Add(commentTrivia);
                    }
                }

                if (baseLeadingTrivia != null)
                {
                    leadingTrivias.Add(baseLeadingTrivia.Value);
                }

                leadingTrivias.Add(SyntaxTriviaHelper.GetTab());

                return obj
                    .WithLeadingTrivia(
                        SyntaxFactory.TriviaList(leadingTrivias)
                    )
                    .WithTrailingTrivia(
                        SyntaxTriviaHelper.GetEndOfLine()
                    );
            });
        return sortedemberDeclarations.ToList();
    }

    private int GetMemberDeclarationModifierRank(
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