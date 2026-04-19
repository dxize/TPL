using Ast;
using Ast.Declarations;

using Semantics.Exceptions;

namespace Semantics.Symbols;

/// <summary>
/// Таблица символов с лексической областью видимости.
/// </summary>
public sealed class SymbolsTable
{
    private readonly SymbolsTable? _parent;
    private readonly Dictionary<string, VariableSymbol> _variables;
    private readonly Dictionary<string, BuiltinSymbol> _builtins;
    private readonly Dictionary<string, FunctionDeclaration> _functions;

    public SymbolsTable(SymbolsTable? parent)
    {
        _parent = parent;
        _variables = new Dictionary<string, VariableSymbol>(StringComparer.Ordinal);
        _builtins = new Dictionary<string, BuiltinSymbol>(StringComparer.Ordinal);
        _functions = new Dictionary<string, FunctionDeclaration>(StringComparer.Ordinal);
    }

    public SymbolsTable? Parent => _parent;

    /// <summary>
    /// Объявить переменную/константу в текущей области.
    /// </summary>
    public void DeclareVariable(string name, DataType type, bool isConst)
    {
        if (_variables.ContainsKey(name))
        {
            throw new DuplicateIdentifierException($"Duplicate identifier '{name}'.");
        }

        _variables[name] = new VariableSymbol(type, isConst);
    }

    /// <summary>
    /// Объявить встроенную функцию в текущей области.
    /// </summary>
    public void DeclareBuiltin(string name, BuiltinInfo info)
    {
        _builtins[name] = new BuiltinSymbol(info);
    }

    /// <summary>
    /// Объявить пользовательскую функцию или процедуру в текущей области.
    /// </summary>
    public void DeclareFunction(FunctionDeclaration function)
    {
        if (_functions.ContainsKey(function.Name))
        {
            throw new DuplicateIdentifierException($"Duplicate callable '{function.Name}'.");
        }

        _functions[function.Name] = function;
    }

    /// <summary>
    /// Найти переменную или константу по имени.
    /// Поднимается по цепочке родительских областей.
    /// </summary>
    public SymbolInfo? ResolveVariable(string name)
    {
        if (_variables.TryGetValue(name, out VariableSymbol? symbol))
        {
            return symbol.ToSymbolInfo();
        }

        return _parent?.ResolveVariable(name);
    }

    /// <summary>
    /// Найти встроенную функцию по имени.
    /// Поднимается по цепочке родительских областей.
    /// </summary>
    public BuiltinInfo? ResolveBuiltin(string name)
    {
        if (_builtins.TryGetValue(name, out BuiltinSymbol? symbol))
        {
            return symbol.Info;
        }

        return _parent?.ResolveBuiltin(name);
    }

    /// <summary>
    /// Найти пользовательскую функцию или процедуру по имени.
    /// Поднимается по цепочке родительских областей.
    /// </summary>
    public FunctionDeclaration? ResolveFunction(string name)
    {
        if (_functions.TryGetValue(name, out FunctionDeclaration? function))
        {
            return function;
        }

        return _parent?.ResolveFunction(name);
    }

    /// <summary>
    /// Проверить, объявлена ли переменная в текущей или родительских областях.
    /// </summary>
    public bool ContainsVariable(string name)
    {
        if (_variables.ContainsKey(name))
        {
            return true;
        }

        return _parent?.ContainsVariable(name) ?? false;
    }

    private sealed class VariableSymbol
    {
        public VariableSymbol(DataType type, bool isConst)
        {
            Type = type;
            IsConst = isConst;
        }

        public DataType Type { get; }

        public bool IsConst { get; }

        public SymbolInfo ToSymbolInfo() => new(Type, IsConst);
    }

    private sealed class BuiltinSymbol
    {
        public BuiltinSymbol(BuiltinInfo info)
        {
            Info = info;
        }

        public BuiltinInfo Info { get; }
    }
}