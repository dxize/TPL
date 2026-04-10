using Ast;

using Semantics.Passes;

namespace Semantics;

/// <summary>
/// Фасад над несколькими семантическими проходами в стиле DEA.
/// </summary>
public sealed class SemanticsChecker
{
    private readonly AbstractPass[] _passes;

    public SemanticsChecker()
    {
        _passes =
        [
            new ResolveNamesPass(),
            new CheckContextSensitiveRulesPass(),
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