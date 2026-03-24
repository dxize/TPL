namespace VirtualMachine.Instructions;

public enum InstructionCode
{
    Push,
    DefineVar,
    LoadVar,
    StoreVar,
    InputVar,
    Add,
    Subtract,
    Multiply,
    Divide,
    IntegerDivide,
    Modulo,
    Power,
    Negate,
    CallBuiltin,
    Halt,
}