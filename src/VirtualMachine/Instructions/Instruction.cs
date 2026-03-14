using Runtime;

namespace VirtualMachine.Instructions;

public sealed class Instruction
{
    public Instruction(InstructionCode code)
    {
        Code = code;
        Operand = Value.Void;
    }

    public Instruction(InstructionCode code, Value operand)
    {
        Code = code;
        Operand = operand;
    }

    public InstructionCode Code { get; }

    public Value Operand { get; }
}
