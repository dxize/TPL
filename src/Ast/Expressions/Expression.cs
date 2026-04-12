namespace Ast.Expressions;

public abstract class Expression : AstNode
{
    private AstAttribute<DataType> _resultType;

    /// <summary>
    /// Тип результата выражения.
    /// </summary>
    public DataType ResultType
    {
        get => _resultType.Get();
        set => _resultType.Set(value);
    }
}