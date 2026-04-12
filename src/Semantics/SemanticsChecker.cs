using Ast;

using Semantics.Passes;
using Semantics.Symbols;

namespace Semantics;

/// <summary>
/// Фасад над несколькими семантическими проходами в стиле DEA.
/// </summary>
public sealed class SemanticsChecker
{
    private readonly AbstractPass[] _passes;

    public SemanticsChecker()
    {
        // Создаём глобальную таблицу символов и заполняем встроенными функциями
        SymbolsTable globalSymbols = new(parent: null);
        globalSymbols.DeclareBuiltin("abs", new BuiltinInfo("abs", 1, DataType.Int));
        globalSymbols.DeclareBuiltin("min", new BuiltinInfo("min", null, DataType.Int));
        globalSymbols.DeclareBuiltin("max", new BuiltinInfo("max", null, DataType.Int));
        globalSymbols.DeclareBuiltin("len", new BuiltinInfo("len", 1, DataType.Int));
        globalSymbols.DeclareBuiltin("substr", new BuiltinInfo("substr", 3, DataType.String));

        _passes =
        [
            new ResolveNamesPass(globalSymbols),
            new CheckContextSensitiveRulesPass(),
            new ResolveTypesPass(),
            new CheckTypesPass(),
        ];
    }

    public void Check(ProgramNode program)
    {
        foreach (AbstractPass pass in _passes)
        {
            program.Accept(pass);
        }
    }
}