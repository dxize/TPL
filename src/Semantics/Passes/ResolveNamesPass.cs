using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Semantics.Exceptions;

namespace Semantics.Passes;

/// <summary>
/// Проход для разрешения имён с поддержкой иерархии областей видимости.
/// </summary>
public sealed class ResolveNamesPass : AbstractPass
{
    /// <summary>
    /// Глобальная область видимости: имя -> isConst
    /// </summary>
    private readonly Dictionary<string, bool> _globalSymbols = new(StringComparer.Ordinal);

    /// <summary>
    /// Текущая область видимости (локальная для функции)
    /// </summary>
    private Dictionary<string, bool>? _currentScope;

    public override void Visit(ProgramNode p)
    {
        _globalSymbols.Clear();
        _currentScope = null;

        // Сначала обходим глобальные объявления
        foreach (Declaration decl in p.GlobalDeclarations)
        {
            decl.Accept(this);
        }

        // Затем обходим main
        p.MainFunction.Accept(this);
    }

    public override void Visit(FunctionDeclaration d)
    {
        if (!string.Equals(d.Name, "main", StringComparison.Ordinal))
        {
            throw new InvalidExpressionException(
                "Поддерживается только точка входа func int main().");
        }

        // Создаём локальную область видимости для функции
        _currentScope = new Dictionary<string, bool>(StringComparer.Ordinal);

        try
        {
            base.Visit(d);
        }
        finally
        {
            _currentScope = null;
        }
    }

    public override void Visit(VariableDeclarationExpression e)
    {
        // Проверяем дубликаты в текущей области
        if (_currentScope != null)
        {
            // Локальное объявление
            if (_currentScope.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Идентификатор '{e.Name}' уже объявлен в текущей области.");
            }

            // Проверяем, нет ли такого имени в глобальной области (скрытие запрещено)
            if (_globalSymbols.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Идентификатор '{e.Name}' уже объявлен в глобальной области.");
            }

            if (e.Initializer is not null)
            {
                e.Initializer.Accept(this);
            }

            _currentScope[e.Name] = false;
        }
        else
        {
            // Глобальное объявление
            if (_globalSymbols.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Идентификатор '{e.Name}' уже объявлен.");
            }

            if (e.Initializer is not null)
            {
                e.Initializer.Accept(this);
            }

            _globalSymbols[e.Name] = false;
        }
    }

    public override void Visit(ConstantDeclarationExpression e)
    {
        // Проверяем дубликаты в текущей области
        if (_currentScope != null)
        {
            // Локальное объявление
            if (_currentScope.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Идентификатор '{e.Name}' уже объявлен в текущей области.");
            }

            // Проверяем, нет ли такого имени в глобальной области (скрытие запрещено)
            if (_globalSymbols.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Идентификатор '{e.Name}' уже объявлен в глобальной области.");
            }

            e.Initializer.Accept(this);
            _currentScope[e.Name] = true;
        }
        else
        {
            // Глобальное объявление
            if (_globalSymbols.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Идентификатор '{e.Name}' уже объявлен.");
            }

            e.Initializer.Accept(this);
            _globalSymbols[e.Name] = true;
        }
    }

    public override void Visit(AssignmentExpression e)
    {
        // Ищем в локальной области, затем в глобальной
        bool found = false;
        bool isConst = false;

        if (_currentScope != null && _currentScope.TryGetValue(e.Name, out bool localIsConst))
        {
            found = true;
            isConst = localIsConst;
        }
        else if (_globalSymbols.TryGetValue(e.Name, out bool globalIsConst))
        {
            found = true;
            isConst = globalIsConst;
        }

        if (!found)
        {
            throw new UnknownIdentifierException($"Идентификатор '{e.Name}' не объявлен.");
        }

        if (isConst)
        {
            throw new InvalidExpressionException($"Нельзя присваивать значение константе '{e.Name}'.");
        }

        e.Value.Accept(this);
    }

    public override void Visit(IdentifierExpression e)
    {
        // Ищем в локальной области, затем в глобальной
        bool found = false;

        if (_currentScope != null && _currentScope.ContainsKey(e.Name))
        {
            found = true;
        }
        else if (_globalSymbols.ContainsKey(e.Name))
        {
            found = true;
        }

        if (!found)
        {
            throw new UnknownIdentifierException($"Идентификатор '{e.Name}' не объявлен.");
        }
    }

    public override void Visit(InputExpression e)
    {
        // Ищем в локальной области, затем в глобальной
        bool found = false;
        bool isConst = false;

        if (_currentScope != null && _currentScope.TryGetValue(e.VariableName, out bool localIsConst))
        {
            found = true;
            isConst = localIsConst;
        }
        else if (_globalSymbols.TryGetValue(e.VariableName, out bool globalIsConst))
        {
            found = true;
            isConst = globalIsConst;
        }

        if (!found)
        {
            throw new UnknownIdentifierException($"Идентификатор '{e.VariableName}' не объявлен.");
        }

        if (isConst)
        {
            throw new InvalidExpressionException($"Нельзя выполнять input в константу '{e.VariableName}'.");
        }
    }

    public override void Visit(CallExpression e)
    {
        if (e.Name is not ("abs" or "min" or "max" or "len" or "substr"))
        {
            throw new UnknownIdentifierException($"Неизвестная функция '{e.Name}'.");
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