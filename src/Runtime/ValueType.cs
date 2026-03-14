namespace Runtime;

public sealed class ValueType
{
    public static readonly ValueType Void = new("void");
    public static readonly ValueType Int = new("int");
    public static readonly ValueType Num = new("num");
    public static readonly ValueType String = new("string");

    private readonly string _name;

    private ValueType(string name)
    {
        _name = name;
    }

    public override string ToString() => _name;
}
