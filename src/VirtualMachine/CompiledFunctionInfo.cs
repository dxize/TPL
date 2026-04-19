using Runtime;

using ValueType = Runtime.ValueType;

namespace VirtualMachine;

public sealed class CompiledFunctionInfo
{
    public CompiledFunctionInfo(string name, int entryPoint, IReadOnlyList<string> parameterNames, ValueType returnType)
    {
        Name = name;
        EntryPoint = entryPoint;
        ParameterNames = parameterNames;
        ReturnType = returnType;
    }

    public string Name { get; }

    public int EntryPoint { get; }

    public IReadOnlyList<string> ParameterNames { get; }

    public ValueType ReturnType { get; }

    public bool IsProcedure => ReturnType == ValueType.Void;
}