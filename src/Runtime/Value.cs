using System.Globalization;

namespace Runtime;

public sealed class Value
{
    public static readonly Value Void = new(VoidValue.Value);

    private readonly object _value;

    public Value(int value)
    {
        _value = value;
        Type = ValueType.Int;
    }

    public Value(double value)
    {
        _value = value;
        Type = ValueType.Num;
    }

    public Value(string value)
    {
        _value = value;
        Type = ValueType.String;
    }

    public Value(bool value)
    {
        _value = value;
        Type = ValueType.Bool;
    }

    private Value(VoidValue value)
    {
        _value = value;
        Type = ValueType.Void;
    }

    public ValueType Type { get; }

    public bool IsVoid() => Type == ValueType.Void;

    public int AsInt() => _value is int value
        ? value
        : throw new InvalidOperationException($"Value '{this}' is not int.");

    public double AsNum() => _value switch
    {
        double value => value,
        int value => value,
        _ => throw new InvalidOperationException($"Value '{this}' is not num."),
    };

    public string AsString() => _value is string value
        ? value
        : throw new InvalidOperationException($"Value '{this}' is not string.");

    public bool AsBool() => _value is bool value
        ? value
        : throw new InvalidOperationException($"Value '{this}' is not bool.");

    public string ToDisplayString()
    {
        return _value switch
        {
            int value => value.ToString(CultureInfo.InvariantCulture),
            double value => value.ToString(CultureInfo.InvariantCulture),
            string value => value,
            bool value => value ? "true" : "false",
            VoidValue => string.Empty,
            _ => throw new InvalidOperationException($"Unsupported runtime value '{_value}'."),
        };
    }

    public override string ToString() => ToDisplayString();
}