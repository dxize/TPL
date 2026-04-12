using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Semantics.Exceptions;
using Semantics.Symbols;

namespace Semantics.Passes;

/// <summary>
/// Проход разрешения имён: записывает resolved-ссылки на узлы AST,
/// управляет областями видимости через SymbolsTable.
/// Все операции разрешения имён выполняются inline в Visit-методах,
/// </summary>
public sealed class ResolveNamesPass : AbstractPass
{
    private static readonly HashSet<string> ReservedNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "int", "num", "string", "bool",
    };

    private SymbolsTable _symbols;

    public ResolveNamesPass(SymbolsTable globalSymbols)
    {
        _symbols = globalSymbols;
    }

    public override void Visit(ProgramNode p)
    {
        foreach (Declaration decl in p.GlobalDeclarations)
        {
            decl.Accept(this);
        }

        p.MainFunction.Accept(this);
    }

    public override void Visit(FunctionDeclaration d)
    {
        if (!string.Equals(d.Name, "main", StringComparison.Ordinal))
        {
            throw new InvalidExpressionException("Only the entry point func int main() is supported.");
        }

        _symbols = new SymbolsTable(_symbols);

        try
        {
            base.Visit(d);
        }
        finally
        {
            _symbols = _symbols.Parent!;
        }
    }

    public override void Visit(VariableDeclarationExpression e)
    {
        EnsureNotReservedName(e.Name);
        EnsureNotBuiltinName(e.Name);

        base.Visit(e);

        if (_symbols.Parent is not null && _symbols.Parent.ContainsVariable(e.Name))
        {
            throw new DuplicateIdentifierException($"Identifier '{e.Name}' is already declared in global scope.");
        }

        _symbols.DeclareVariable(e.Name, e.Type, isConst: false);
    }

    public override void Visit(ConstantDeclarationExpression e)
    {
        EnsureNotReservedName(e.Name);
        EnsureNotBuiltinName(e.Name);

        base.Visit(e);

        if (_symbols.Parent is not null && _symbols.Parent.ContainsVariable(e.Name))
        {
            throw new DuplicateIdentifierException($"Identifier '{e.Name}' is already declared in global scope.");
        }

        _symbols.DeclareVariable(e.Name, e.Type, isConst: true);
    }

    public override void Visit(IdentifierExpression e)
    {
        base.Visit(e);

        SymbolInfo? symbol = _symbols.ResolveVariable(e.Name);

        if (symbol is null)
        {
            throw new UnknownIdentifierException($"Identifier '{e.Name}' is not declared.");
        }

        e.Symbol = symbol;
    }

    public override void Visit(AssignmentExpression e)
    {
        SymbolInfo? symbol = _symbols.ResolveVariable(e.Name);

        if (symbol is null)
        {
            throw new UnknownIdentifierException($"Identifier '{e.Name}' is not declared.");
        }

        if (symbol.IsConst)
        {
            throw new InvalidAssignmentException($"Cannot assign to constant '{e.Name}'.");
        }

        e.Symbol = symbol;
        base.Visit(e);
    }

    public override void Visit(InputExpression e)
    {
        SymbolInfo? symbol = _symbols.ResolveVariable(e.VariableName);

        if (symbol is null)
        {
            throw new UnknownIdentifierException($"Cannot read input into undeclared variable '{e.VariableName}'.");
        }

        if (symbol.IsConst)
        {
            throw new InvalidAssignmentException($"Cannot read input into constant '{e.VariableName}'.");
        }

        e.Symbol = symbol;
        base.Visit(e);
    }

    public override void Visit(CallExpression e)
    {
        base.Visit(e);

        BuiltinInfo? builtin = _symbols.ResolveBuiltin(e.Name);

        if (builtin is null)
        {
            throw new UnknownIdentifierException($"Unknown function '{e.Name}'.");
        }

        e.Builtin = builtin;
    }

    public override void Visit(PrintExpression e)
    {
        base.Visit(e);
    }

    public override void Visit(ReturnExpression e)
    {
        base.Visit(e);
    }

    public override void Visit(LiteralExpression e)
    {
    }

    private static void EnsureNotReservedName(string name)
    {
        if (ReservedNames.Contains(name))
        {
            throw new DuplicateIdentifierException($"Identifier '{name}' is a reserved name.");
        }
    }

    private void EnsureNotBuiltinName(string name)
    {
        if (_symbols.ResolveBuiltin(name) is not null)
        {
            throw new DuplicateIdentifierException($"Identifier '{name}' is a reserved builtin name.");
        }
    }
}