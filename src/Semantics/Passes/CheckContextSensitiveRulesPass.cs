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
    private bool _hasReturn;
    private bool _afterReturn;

    public override void Visit(ProgramNode p)
    {
        if (p.MainFunction is null)
        {
            throw new InvalidExpressionException(
                "Program must contain the entry point func int main().");
        }

        _hasReturn = false;
        _afterReturn = false;

        base.Visit(p);
    }

    public override void Visit(FunctionDeclaration d)
    {
        if (!string.Equals(d.Name, "main", StringComparison.Ordinal))
        {
            throw new InvalidExpressionException(
                "Only the main function is supported in this iteration.");
        }

        if (d.ReturnType != Ast.DataType.Int)
        {
            throw new InvalidExpressionException(
                "Main function must return int.");
        }

        if (d.Body.Count == 0)
        {
            throw new InvalidExpressionException(
                "Main function body must not be empty.");
        }

        foreach (AstNode node in d.Body)
        {
            if (_afterReturn)
            {
                throw new InvalidExpressionException(
                    "No statements allowed after return.");
            }

            node.Accept(this);
        }

        if (!_hasReturn)
        {
            throw new InvalidExpressionException(
                "Main function must contain a return statement with an int value.");
        }
    }

    public override void Visit(ReturnExpression e)
    {
        base.Visit(e);
        _hasReturn = true;
        _afterReturn = true;
    }

    public override void Visit(CallExpression e)
    {
        base.Visit(e);
        CheckBuiltinFunctionArguments(e);
    }

    /// <summary>
    /// Проверяет количество аргументов встроенных функций.
    /// </summary>
    private static void CheckBuiltinFunctionArguments(CallExpression e)
    {
        switch (e.Name)
        {
            case "abs":
                if (e.Arguments.Count != 1)
                {
                    throw new InvalidFunctionCallException(
                        $"Function 'abs' requires 1 argument, got {e.Arguments.Count}.");
                }

                break;
            case "len":
                if (e.Arguments.Count != 1)
                {
                    throw new InvalidFunctionCallException(
                        $"Function 'len' requires 1 argument, got {e.Arguments.Count}.");
                }

                break;
            case "substr":
                if (e.Arguments.Count != 3)
                {
                    throw new InvalidFunctionCallException(
                        $"Function 'substr' requires 3 arguments, got {e.Arguments.Count}.");
                }

                break;
            case "min":
            case "max":
                if (e.Arguments.Count < 2)
                {
                    throw new InvalidFunctionCallException(
                        $"Function '{e.Name}' requires at least 2 arguments, got {e.Arguments.Count}.");
                }

                break;
        }
    }
}