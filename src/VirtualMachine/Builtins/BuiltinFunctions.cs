using Runtime;

using Semantics.Exceptions;

namespace VirtualMachine.Builtins;

public sealed class BuiltinFunctions
{
    private readonly IEnvironment _environment;

    public BuiltinFunctions(IEnvironment environment)
    {
        _environment = environment;
    }

    public void Print(Value value)
    {
        _environment.Print(value.ToDisplayString());
    }

    public Value Len(Value value)
    {
        return new Value(value.AsString().Length);
    }

    public Value Substr(Value source, Value start, Value length)
    {
        string text = source.AsString();
        int startIndex = start.AsInt();
        int sliceLength = length.AsInt();

        if (startIndex < 0 || sliceLength < 0 || startIndex > text.Length || startIndex + sliceLength > text.Length)
        {
            throw new RuntimeException("Invalid substr bounds.");
        }

        return new Value(text.Substring(startIndex, sliceLength));
    }

    public Value Abs(Value value)
    {
        if (value.Type == Runtime.ValueType.Int)
        {
            return new Value(Math.Abs(value.AsInt()));
        }

        if (value.Type == Runtime.ValueType.Num)
        {
            return new Value(Math.Abs(value.AsNum()));
        }

        throw new RuntimeException("abs supports only int or num.");
    }
}