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
                "Программа должна содержать точку входа func int main().");
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
                "Во 2-й итерации поддерживается только функция main.");
        }

        if (d.ReturnType != Ast.DataType.Int)
        {
            throw new InvalidExpressionException(
                "Функция main должна возвращать int.");
        }

        if (d.Body.Count == 0)
        {
            throw new InvalidExpressionException(
                "Тело функции main не должно быть пустым.");
        }

        foreach (AstNode node in d.Body)
        {
            if (_afterReturn)
            {
                throw new InvalidExpressionException(
                    "После return не должно быть инструкций.");
            }

            node.Accept(this);
        }

        if (!_hasReturn)
        {
            throw new InvalidExpressionException(
                "Функция main должна содержать return выражение типа int.");
        }
    }

    public override void Visit(ReturnExpression e)
    {
        base.Visit(e);
        _hasReturn = true;
        _afterReturn = true;
    }
}