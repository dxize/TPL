using VirtualMachine.Instructions;

namespace VirtualMachine;

public sealed class CompiledProgram
{
    public CompiledProgram(IReadOnlyList<Instruction> instructions, IReadOnlyDictionary<string, CompiledFunctionInfo> functions)
    {
        Instructions = instructions;
        Functions = functions;
    }

    public IReadOnlyList<Instruction> Instructions { get; }

    public IReadOnlyDictionary<string, CompiledFunctionInfo> Functions { get; }
}