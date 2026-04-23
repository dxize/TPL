using Runtime;

using ValueType = Runtime.ValueType;

namespace VirtualMachine;

public sealed class CompiledFunctionInfo
{
    public CompiledFunctionInfo(
        string name,
        int entryPoint,
        IReadOnlyList<string> parameterNames,
        ValueType returnType)
        : this(name, entryPoint, parameterNames, Array.Empty<ValueType>(), returnType)
    {
    }

    public CompiledFunctionInfo(
        string name,
        int entryPoint,
        IReadOnlyList<string> parameterNames,
        IReadOnlyList<ValueType> parameterTypes,
        ValueType returnType)
    {
        Name = name;
        EntryPoint = entryPoint;
        ParameterNames = parameterNames;
        ParameterTypes = parameterTypes;
        ReturnType = returnType;
    }

    public string Name { get; }

    public int EntryPoint { get; }

    public IReadOnlyList<string> ParameterNames { get; }

    public IReadOnlyList<ValueType> ParameterTypes { get; }

    public ValueType ReturnType { get; }

    public bool IsProcedure => ReturnType == ValueType.Void;
}