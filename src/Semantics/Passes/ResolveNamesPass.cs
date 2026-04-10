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
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        // Встроенные функции
        "abs", "min", "max", "len", "substr",

        // Типы данных
        "int", "num", "string", "bool",
    };

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
                "Only the entry point func int main() is supported.");
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
        EnsureNotReservedName(e.Name);

        if (_currentScope != null)
        {
            if (_currentScope.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Duplicate identifier '{e.Name}' in current scope.");
            }

            if (_globalSymbols.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Identifier '{e.Name}' is already declared in global scope.");
            }

            if (e.Initializer is not null)
            {
                e.Initializer.Accept(this);
            }

            _currentScope[e.Name] = false;
        }
        else
        {
            if (_globalSymbols.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Duplicate identifier '{e.Name}'.");
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
        EnsureNotReservedName(e.Name);

        if (_currentScope != null)
        {
            if (_currentScope.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Duplicate identifier '{e.Name}' in current scope.");
            }

            if (_globalSymbols.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Identifier '{e.Name}' is already declared in global scope.");
            }

            e.Initializer.Accept(this);
            _currentScope[e.Name] = true;
        }
        else
        {
            if (_globalSymbols.ContainsKey(e.Name))
            {
                throw new DuplicateIdentifierException($"Duplicate identifier '{e.Name}'.");
            }

            e.Initializer.Accept(this);
            _globalSymbols[e.Name] = true;
        }
    }

    public override void Visit(AssignmentExpression e)
    {
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
            throw new UnknownIdentifierException($"Identifier '{e.Name}' is not declared.");
        }

        if (isConst)
        {
            throw new InvalidExpressionException($"Cannot assign to constant '{e.Name}'.");
        }

        e.Value.Accept(this);
    }

    public override void Visit(IdentifierExpression e)
    {
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
            throw new UnknownIdentifierException($"Identifier '{e.Name}' is not declared.");
        }
    }

    public override void Visit(InputExpression e)
    {
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
            throw new UnknownIdentifierException($"Identifier '{e.VariableName}' is not declared.");
        }

        if (isConst)
        {
            throw new InvalidExpressionException($"Cannot read input into constant '{e.VariableName}'.");
        }
    }

    public override void Visit(CallExpression e)
    {
        if (e.Name is not ("abs" or "min" or "max" or "len" or "substr"))
        {
            throw new UnknownIdentifierException($"Unknown function '{e.Name}'.");
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

    private static void EnsureNotReservedName(string name)
    {
        if (ReservedNames.Contains(name))
        {
            throw new DuplicateIdentifierException($"Identifier '{name}' is a reserved name.");
        }
    }
}