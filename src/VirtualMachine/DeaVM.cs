using Runtime;
using VirtualMachine.Builtins;
using VirtualMachine.Instructions;

namespace VirtualMachine;

public sealed class DeaVM
{
    private readonly IReadOnlyList<Instruction> _instructions;
    private readonly BuiltinFunctions _builtins;
    private readonly Stack<Value> _evaluationStack;
    private int _instructionPointer;

    public DeaVM(IEnvironment environment, IReadOnlyList<Instruction> instructions)
    {
        _instructions = instructions;
        _builtins = new BuiltinFunctions(environment);
        _evaluationStack = [];
        _instructionPointer = 0;
    }

    public int RunProgram()
    {
        while (true)
        {
            Instruction instruction = _instructions[_instructionPointer++];
            switch (instruction.Code)
            {
                case InstructionCode.Push:
                    _evaluationStack.Push(instruction.Operand);
                    break;

                case InstructionCode.CallBuiltin:
                    ExecuteBuiltin(instruction.Operand);
                    break;

                case InstructionCode.Halt:
                    return _evaluationStack.Pop().AsInt();

                default:
                    throw new InvalidOperationException($"Unsupported instruction '{instruction.Code}'.");
            }
        }
    }

    private void ExecuteBuiltin(Value operand)
    {
        BuiltinFunctionCode builtin = (BuiltinFunctionCode)operand.AsInt();
        switch (builtin)
        {
            case BuiltinFunctionCode.Print:
                _builtins.Print(_evaluationStack.Pop());
                break;

            default:
                throw new InvalidOperationException($"Unsupported builtin '{builtin}'.");
        }
    }
}