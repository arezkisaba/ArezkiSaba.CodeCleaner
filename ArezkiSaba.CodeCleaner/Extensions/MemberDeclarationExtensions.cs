using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Data.Common;
using System.Reflection.Metadata;

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

    public static MemberDeclarationSyntax RemoveAllTriviasFromParametersAndArguments(
        this MemberDeclarationSyntax declaration)
    {
        var newDeclaration = declaration;

        var parameterLists = newDeclaration.DescendantNodes().OfType<ParameterListSyntax>().ToList();
        newDeclaration = newDeclaration.ReplaceNodes(parameterLists, (parameterList, __) =>
        {
            var newParameters = parameterList.Parameters.Select((parameter, index) =>
            {
                return parameter.WithoutLeadingTrivia().WithoutTrailingTrivia();
            }).ToList();
            var newParametersList = parameterList.WithParameters(SyntaxFactory.SeparatedList(newParameters));
            newParametersList = newParametersList.WithOpenParenToken(newParametersList.OpenParenToken.WithoutTrivia());
            newParametersList = newParametersList.WithCloseParenToken(newParametersList.CloseParenToken.WithoutTrivia());
            return newParametersList;
        });

        var argumentLists = newDeclaration.DescendantNodes().OfType<ArgumentListSyntax>().ToList();
        newDeclaration = newDeclaration.ReplaceNodes(argumentLists, (argumentList, __) =>
        {
            var newArguments = argumentList.Arguments.Select((argument, index) =>
            {
                return argument.WithoutLeadingTrivia().WithoutTrailingTrivia();
            }).ToList();
            var newArgumentList = argumentList.WithArguments(SyntaxFactory.SeparatedList(newArguments));
            newArgumentList = newArgumentList.WithOpenParenToken(newArgumentList.OpenParenToken.WithoutTrivia());
            newArgumentList = newArgumentList.WithCloseParenToken(newArgumentList.CloseParenToken.WithoutTrivia());
            return newArgumentList;
        });

        return newDeclaration;
    }
}