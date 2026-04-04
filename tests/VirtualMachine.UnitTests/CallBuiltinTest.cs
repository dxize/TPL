using Runtime;
using Semantics.Exceptions;
using TestsLibrary;
using VirtualMachine.Builtins;
using VirtualMachine.Instructions;

namespace VirtualMachine.UnitTests;

public class CallBuiltinTest
{
    [Theory]
    [MemberData(nameof(GetInputOutputData))]
    public void Can_use_input_and_output(
        IReadOnlyList<Instruction> program,
        string input,
        string expectedOutput
    )
    {
        FakeEnvironment environment = new();
        if (!string.IsNullOrEmpty(input))
        {
            environment.AddInput(input); // Предполагаем, что у тебя есть этот метод
        }

        DeaVM vm = new(environment, program);
        int exitCode = vm.RunProgram();

        Assert.Equal(0, exitCode);
        Assert.Equal(expectedOutput, environment.Output);
    }

    public static TheoryData<IReadOnlyList<Instruction>, string, string> GetInputOutputData()
    {
        return new TheoryData<IReadOnlyList<Instruction>, string, string>
        {
            // Функция print
            {
                [
                    new Instruction(InstructionCode.Push, new Value("hello")),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ],
                "", "hello"
            },

            // Функция input (считываем "42" и печатаем его)
            {
                [
                    new Instruction(InstructionCode.Push, Value.Void),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.DefineVar, new Value("x")),
                    new Instruction(InstructionCode.InputVar, new Value("x")),
                    new Instruction(InstructionCode.LoadVar, new Value("x")),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ],
                "42", "42"
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetBuiltinFunctionsData))]
    public void Can_call_builtin_functions(
        IReadOnlyList<Instruction> program,
        string expectedOutput
    )
    {
        FakeEnvironment environment = new();
        DeaVM vm = new(environment, program);

        vm.RunProgram();

        Assert.Equal(expectedOutput, environment.Output);
    }

    public static TheoryData<IReadOnlyList<Instruction>, string> GetBuiltinFunctionsData()
    {
        return new TheoryData<IReadOnlyList<Instruction>, string>
        {
            // Функция len: len("abcde") = 5
            {
                [
                    new Instruction(InstructionCode.Push, new Value("abcde")),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Len)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ],
                "5"
            },

            // Функция substr: substr("abcde", 1, 3) = "bcd"
            {
                [
                    new Instruction(InstructionCode.Push, new Value("abcde")),
                    new Instruction(InstructionCode.Push, new Value(1)),
                    new Instruction(InstructionCode.Push, new Value(3)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Substr)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ],
                "bcd"
            },

            // Функция abs: abs(-7) = 7
            {
                [
                    new Instruction(InstructionCode.Push, new Value(-7)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Abs)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ],
                "7"
            },

            // Функция max (3 аргумента): max(3, 5, 2) = 5
            {
                [
                    new Instruction(InstructionCode.Push, new Value(3)),
                    new Instruction(InstructionCode.Push, new Value(5)),
                    new Instruction(InstructionCode.Push, new Value(2)),
                    new Instruction(InstructionCode.Push, new Value(3)), // количество аргументов
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Max)),
                    new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ],
                "5"
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidBuiltinData))]
    public void Throws_for_invalid_builtin_usage(IReadOnlyList<Instruction> program)
    {
        FakeEnvironment environment = new();
        DeaVM vm = new(environment, program);
        Assert.Throws<RuntimeException>(() => vm.RunProgram());
    }

    public static TheoryData<IReadOnlyList<Instruction>> GetInvalidBuiltinData()
    {
        return
        [

            [
                new Instruction(InstructionCode.Push, new Value("abc")),
                new Instruction(InstructionCode.Push, new Value(2)),
                new Instruction(InstructionCode.Push, new Value(5)),
                new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Substr)),
                new Instruction(InstructionCode.Halt),
            ]
        ];
    }
}