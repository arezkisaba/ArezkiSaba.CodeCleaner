using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ArezkiSaba.CodeCleaner.Extensions;

public static class ControlStructureStatementExtensions
{
    public static StatementSyntax AddBracesBasedOnParent(
        this IfStatementSyntax statement,
        SyntaxNode parentNode)
    {
        if (statement.Statement is not BlockSyntax)
        {
            var newBlock = SyntaxFactory.Block(statement.Statement);
            return statement.WithStatement(newBlock);
        }

        return statement;
    }

    public static ForStatementSyntax AddBracesBasedOnParent(
        this ForStatementSyntax statement,
        SyntaxNode parentNode)
    {
        if (statement.Statement is not BlockSyntax)
        {
            var newBlock = SyntaxFactory.Block(statement.Statement);
            return statement.WithStatement(newBlock);
        }

        return statement;
    }

    public static WhileStatementSyntax AddBracesBasedOnParent(
        this WhileStatementSyntax statement,
        SyntaxNode parentNode)
    {
        if (statement.Statement is not BlockSyntax)
        {
            var newBlock = SyntaxFactory.Block(statement.Statement);
            return statement.WithStatement(newBlock);
        }

        return statement;
    }
}
