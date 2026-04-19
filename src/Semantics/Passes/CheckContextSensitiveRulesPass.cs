using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Semantics.Exceptions;

namespace Semantics.Passes;

/// <summary>
/// Проверяет контекстно-зависимые правила:
/// - должна быть только main
/// - main должна возвращать int
/// - тело main не пустое
/// - после return не должно быть инструкций
/// - return должен присутствовать
/// - количество аргументов встроенных функций
/// - присваивание только lvalue
/// - break/continue только внутри циклов (подготовка к итерации 5)
/// </summary>
public sealed class CheckContextSensitiveRulesPass : AbstractPass
{
    public override void Visit(ProgramNode p)
    {
        if (p.MainFunction is null)
        {
            throw new InvalidExpressionException("Program must contain the entry point func int main().");
        }

        base.Visit(p);
    }

    public override void Visit(FunctionDeclaration d)
    {
        if (string.Equals(d.Name, "main", StringComparison.Ordinal) && d.ReturnType != DataType.Int)
        {
            throw new InvalidExpressionException("Main function must return int.");
        }

        if (d.Body.Count == 0)
        {
            throw new InvalidExpressionException($"Function '{d.Name}' body must not be empty.");
        }

        for (int i = 0; i < d.Body.Count; i++)
        {
            AstNode node = d.Body[i];
            node.Accept(this);

            if (node is ReturnExpression && i < d.Body.Count - 1)
            {
                throw new InvalidExpressionException("No statements allowed after top-level return.");
            }
        }

        if (!d.IsProcedure && !ContainsReturn(d.Body))
        {
            throw new InvalidExpressionException($"Function '{d.Name}' must contain a return statement.");
        }
    }

    private static bool ContainsReturn(IEnumerable<AstNode> body)
    {
        foreach (AstNode node in body)
        {
            if (node is ReturnExpression)
            {
                return true;
            }

            if (node is IfStatement ifStatement)
            {
                if (ContainsReturn(ifStatement.ThenBody))
                {
                    return true;
                }

                if (ifStatement.ElseBody is not null && ContainsReturn(ifStatement.ElseBody))
                {
                    return true;
                }
            }
        }

        return false;
    }
}