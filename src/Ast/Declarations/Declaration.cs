namespace Ast.Declarations;

/// <summary>
/// Абстрактный класс всех объявлений (declarations).
/// </summary>
public abstract class Declaration : AstNode
{
    private AstAttribute<DataType> _resultType;

    /// <summary>
    /// Тип объявления (устанавливается на этапе семантического анализа).
    /// </summary>
    public DataType ResultType
    {
        get => _resultType.Get();
        set => _resultType.Set(value);
    }
}