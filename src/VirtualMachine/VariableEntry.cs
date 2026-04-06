using Runtime;

using RuntimeValueType = Runtime.ValueType;

namespace VirtualMachine;

internal sealed class VariableEntry
{
    public VariableEntry(RuntimeValueType type, bool isConst, bool isInitialized, Value value)
    {
        Type = type;
        IsConst = isConst;
        IsInitialized = isInitialized;
        Value = value;
    }

    public RuntimeValueType Type { get; }

    public bool IsConst { get; }

    public bool IsInitialized { get; private set; }

    public Value Value { get; private set; }

    public void SetValue(Value value)
    {
        Value = value;
        IsInitialized = true;
    }
}