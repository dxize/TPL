using Ast;
using Ast.Declarations;
using Ast.Expressions;

namespace Semantics.Passes;

/// <summary>
/// Во 2-й итерации почти нечего резолвить по именам:
/// есть только main, литералы, print и return.
/// Проход оставлен для соответствия шаблону DEA.
/// </summary>
public sealed class ResolveNamesPass : AbstractPass
{
    private readonly Dictionary<string, bool> _symbols = new(StringComparer.Ordinal);

    public override void Visit(ProgramNode p)
    {
        _symbols.Clear();
        base.Visit(p);
    }

    public override void Visit(FunctionDeclaration d)
    {
        if (!string.Equals(d.Name, "main", StringComparison.Ordinal))
        {
            throw new Exceptions.InvalidExpressionException(
                "Во 2-й итерации поддерживается только точка входа func int main().");
        }

        base.Visit(d);
    }

    public override void Visit(VariableDeclarationExpression e)
    {
        if (_symbols.ContainsKey(e.Name))
        {
            throw new Exceptions.InvalidExpressionException($"Идентификатор '{e.Name}' уже объявлен.");
        }

        if (e.Initializer is not null)
        {
            e.Initializer.Accept(this);
        }

        _symbols[e.Name] = false;
    }

    public override void Visit(ConstantDeclarationExpression e)
    {
        if (_symbols.ContainsKey(e.Name))
        {
            throw new Exceptions.InvalidExpressionException($"Идентификатор '{e.Name}' уже объявлен.");
        }

        e.Initializer.Accept(this);
        _symbols[e.Name] = true;
    }

    public override void Visit(AssignmentExpression e)
    {
        if (!_symbols.TryGetValue(e.Name, out bool isConst))
        {
            throw new Exceptions.InvalidExpressionException($"Идентификатор '{e.Name}' не объявлен.");
        }

        if (isConst)
        {
            throw new Exceptions.InvalidExpressionException($"Нельзя присваивать значение константе '{e.Name}'.");
        }

        e.Value.Accept(this);
    }

    public override void Visit(IdentifierExpression e)
    {
        if (!_symbols.ContainsKey(e.Name))
        {
            throw new Exceptions.InvalidExpressionException($"Идентификатор '{e.Name}' не объявлен.");
        }
    }

    public override void Visit(InputExpression e)
    {
        if (!_symbols.TryGetValue(e.VariableName, out bool isConst))
        {
            throw new Exceptions.InvalidExpressionException($"Идентификатор '{e.VariableName}' не объявлен.");
        }

        if (isConst)
        {
            throw new Exceptions.InvalidExpressionException($"Нельзя выполнять input в константу '{e.VariableName}'.");
        }
    }

    public override void Visit(CallExpression e)
    {
        if (e.Name is not ("abs" or "min" or "max" or "len" or "substr"))
        {
            throw new Exceptions.InvalidExpressionException($"Неизвестная функция '{e.Name}'.");
        }

        base.Visit(e);
    }

    public override void Visit(PrintExpression e)
    {
        base.Visit(e);
    }

    public override void Visit(ReturnExpression e)
    {
        base.Visit(e);
    }
}