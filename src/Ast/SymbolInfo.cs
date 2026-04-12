namespace Ast;

/// <summary>
/// Информация о символе, записываемая на узел AST при разрешении имён.
/// </summary>
public sealed class SymbolInfo
{
    public SymbolInfo(DataType type, bool isConst)
    {
        Type = type;
        IsConst = isConst;
    }

    public DataType Type { get; }

    public bool IsConst { get; }
}