using Ast;

using Semantics.Exceptions;

namespace Semantics.Symbols;

/// <summary>
/// Таблица символов с лексической областью видимости (parent-linked chain).
/// </summary>
public sealed class SymbolsTable
{
    private readonly SymbolsTable? _parent;
    private readonly Dictionary<string, VariableSymbol> _variables;
    private readonly Dictionary<string, BuiltinSymbol> _builtins;

    public SymbolsTable(SymbolsTable? parent)
    {
        _parent = parent;
        _variables = new Dictionary<string, VariableSymbol>(StringComparer.Ordinal);
        _builtins = new Dictionary<string, BuiltinSymbol>(StringComparer.Ordinal);
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
    /// Найти символ по имени (сначала переменные, потом builtins).
    /// Поднимается по цепочке родительских scope'ов.
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
    /// Проверить, объявлено ли имя переменной в текущей или родительских областях.
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