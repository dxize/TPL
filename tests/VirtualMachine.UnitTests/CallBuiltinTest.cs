using Runtime;
using Semantics.Exceptions;
using TestsLibrary;
using VirtualMachine.Builtins;
using VirtualMachine.Instructions;

namespace VirtualMachine.UnitTests;

public class CallBuiltinTest
{
    [Theory]
    [MemberData(nameof(GetCallBuiltinData))]
    public void Can_call_builtins(
        IReadOnlyList<Instruction> program,
        string expectedOutput,
        int expectedExitCode
    )
    {
        FakeEnvironment environment = new();
        DeaVM vm = new(environment, program);

        int result = vm.RunProgram();
        Assert.Equal(expectedExitCode, result);
        Assert.Equal(expectedOutput, environment.Output);
    }

    public static TheoryData<IReadOnlyList<Instruction>, string, int> GetCallBuiltinData()
    {
        return new TheoryData<IReadOnlyList<Instruction>, string, int>
        {
            // print("hello")
            {
                [
                    new Instruction(InstructionCode.Push, new Value("hello")),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ],
                "hello",
                0
            },
            // len("abcde"), substr("abcde", 1, 3)
            {
                [
                    new Instruction(InstructionCode.Push, new Value("abcde")),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Len)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),

                    new Instruction(InstructionCode.Push, new Value("abcde")),
                    new Instruction(InstructionCode.Push, new Value(1)),
                    new Instruction(InstructionCode.Push, new Value(3)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Substr)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),

                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ],
                "5bcd",
                0
            },
            // abs(-7), min(3,5,2), max(3,5,2)
            {
                [
                    new Instruction(InstructionCode.Push, new Value(-7)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Abs)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),

                    new Instruction(InstructionCode.Push, new Value(3)),
                    new Instruction(InstructionCode.Push, new Value(5)),
                    new Instruction(InstructionCode.Push, new Value(2)),
                    new Instruction(InstructionCode.Push, new Value(3)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Min)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),

                    new Instruction(InstructionCode.Push, new Value(3)),
                    new Instruction(InstructionCode.Push, new Value(5)),
                    new Instruction(InstructionCode.Push, new Value(2)),
                    new Instruction(InstructionCode.Push, new Value(3)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Max)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),

                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ],
                "725",
                0
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidBuiltinData))]
    public void Throws_for_invalid_builtin_usage(IReadOnlyList<Instruction> program)
    {
        FakeEnvironment environment = new();
        DeaVM vm = new(environment, program);
        Assert.Throws<RuntimeExceptionException>(() => vm.RunProgram());
    }

    public static TheoryData<IReadOnlyList<Instruction>> GetInvalidBuiltinData()
    {
        return new TheoryData<IReadOnlyList<Instruction>>
        {
            // substr: start + length > len
            {
                [
                    new Instruction(InstructionCode.Push, new Value("abc")),
                    new Instruction(InstructionCode.Push, new Value(2)),
                    new Instruction(InstructionCode.Push, new Value(5)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Substr)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ]
            },
            // min/max требуют минимум 2 аргумента
            {
                [
                    new Instruction(InstructionCode.Push, new Value(10)),
                    new Instruction(InstructionCode.Push, new Value(1)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Min)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ]
            },
        };
    }
}
