using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class MemberDeclarationExtensions
{
    public static string GetName(
        this MemberDeclarationSyntax declaration)
    {
        switch (declaration)
        {
            case EventFieldDeclarationSyntax eventField:
                return eventField.Declaration.Variables.FirstOrDefault() != null ? eventField.Declaration.Variables.First().Identifier.Text : null;
            case FieldDeclarationSyntax field:
                return field.Declaration.Variables.FirstOrDefault() != null ? field.Declaration.Variables.First().Identifier.Text : null;
            case PropertyDeclarationSyntax property:
                return property.Identifier.Text;
            case ConstructorDeclarationSyntax constructor:
                return constructor.Identifier.Text;
            case MethodDeclarationSyntax method:
                return method.Identifier.Text;
            default:
                return string.Empty;
        }
    }

    public static MemberDeclarationSyntax FormatByDefault(
        this MemberDeclarationSyntax declaration)
    {
        var newDeclaration = declaration;

        ////var parameterLists = newDeclaration.DescendantNodes().OfType<ParameterListSyntax>().ToList();
        ////newDeclaration = newDeclaration.ReplaceNodes(parameterLists, (parameterList, __) =>
        ////{
        ////    return FormatParametersByDefault(newDeclaration, parameterList);
        ////});

        ////var argumentLists = newDeclaration.DescendantNodes().OfType<ArgumentListSyntax>().ToList();
        ////newDeclaration = newDeclaration.ReplaceNodes(argumentLists, (argumentList, __) =>
        ////{
        ////    return FormatArgumentsByDefault(argumentList);
        ////});

        var constructorInitializers = newDeclaration.DescendantNodes().OfType<ConstructorInitializerSyntax>().ToList();
        newDeclaration = newDeclaration.ReplaceNodes(constructorInitializers, (constructorInitializer, __) =>
        {
            var leadingTrivias = declaration.GetLeadingTrivia().Where(obj => obj.IsKind(SyntaxKind.WhitespaceTrivia)).ToList();
            return constructorInitializer.WithColonToken(constructorInitializer.ColonToken.WithLeadingTrivia(leadingTrivias));
        });

        return newDeclaration;
    }

    #region Private use

    private static SyntaxNode FormatParametersByDefault(
        MemberDeclarationSyntax newDeclaration,
        ParameterListSyntax parameterList)
    {
        var newParameters = parameterList.Parameters.Select((parameter, index) =>
        {
            if (index == 0)
            {
                return parameter.WithoutLeadingTrivia().WithoutTrailingTrivia();
            }

            return parameter.WithLeadingTrivia(SyntaxTriviaHelper.GetWhitespace()).WithoutTrailingTrivia();
        }).ToList();
        var newParametersList = parameterList.WithParameters(SyntaxFactory.SeparatedList(newParameters));
        newParametersList = newParametersList.WithOpenParenToken(newParametersList.OpenParenToken.WithoutTrivia());

        if (newDeclaration.HasBaseOrThisInitializer())
        {
            newParametersList = newParametersList.WithCloseParenToken(
                newParametersList.CloseParenToken.WithoutLeadingTrivias().WithTrailingTrivia(
                    SyntaxFactory.TriviaList(
                        SyntaxTriviaHelper.GetEndOfLine(),
                        newDeclaration.FindFirstLeadingTrivia().Value,
                        SyntaxTriviaHelper.GetTab()
                    )
                )
            );
        }
        else if (!newDeclaration.HasDeclaration())
        {
            newParametersList = newParametersList.WithCloseParenToken(
                newParametersList.CloseParenToken.WithoutLeadingTrivias().WithTrailingTrivia()
            );
        }
        else
        {
            newParametersList = newParametersList.WithCloseParenToken(
                newParametersList.CloseParenToken.WithoutLeadingTrivias().WithTrailingTrivia(
                    SyntaxTriviaHelper.GetEndOfLine()
                )
            );
        }

        return newParametersList;
    }

    private static SyntaxNode FormatArgumentsByDefault(
        ArgumentListSyntax argumentList)
    {
        var newArguments = argumentList.Arguments.Select((argument, index) =>
        {
            if (index == 0)
            {
                return argument.WithoutLeadingTrivia().WithoutTrailingTrivia();
            }

            return argument.WithLeadingTrivia(SyntaxTriviaHelper.GetWhitespace()).WithoutTrailingTrivia();
        }).ToList();
        var newArgumentList = argumentList.WithArguments(SyntaxFactory.SeparatedList(newArguments));
        newArgumentList = newArgumentList.WithOpenParenToken(newArgumentList.OpenParenToken.WithoutTrivia());

        if (argumentList.Parent.IsKind(SyntaxKind.BaseConstructorInitializer)
        || argumentList.Parent.IsKind(SyntaxKind.ThisConstructorInitializer))
        {
            newArgumentList = newArgumentList.WithCloseParenToken(
                newArgumentList.CloseParenToken.WithoutLeadingTrivias().WithTrailingTrivia(SyntaxTriviaHelper.GetEndOfLine())
            );
        }
        else
        {
            newArgumentList = newArgumentList.WithCloseParenToken(
                newArgumentList.CloseParenToken.WithoutLeadingTrivias()
            );
        }

        return newArgumentList;
    }

    #endregion
}
