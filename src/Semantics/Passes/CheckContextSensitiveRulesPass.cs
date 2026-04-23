using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Semantics.Exceptions;

namespace Semantics.Passes;

/// <summary>
/// Проверяет контекстно-зависимые правила:
/// - должна быть точка входа main
/// - main должна возвращать int
/// - тело функции не должно быть пустым
/// - после top-level return не должно быть инструкций
/// - функция должна возвращать значение на всех путях выполнения
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

        if (!d.IsProcedure && !AllPathsReturn(d.Body))
        {
            throw new InvalidExpressionException($"Function '{d.Name}' must return a value on all execution paths.");
        }
    }

    private static bool AllPathsReturn(IEnumerable<AstNode> body)
    {
        foreach (AstNode node in body)
        {
            if (node is ReturnExpression)
            {
                return true;
            }

            if (node is IfStatement ifStatement)
            {
                if (ifStatement.ElseBody is not null
                    && AllPathsReturn(ifStatement.ThenBody)
                    && AllPathsReturn(ifStatement.ElseBody))
                {
                    return true;
                }
            }
        }

        return false;
    }
}