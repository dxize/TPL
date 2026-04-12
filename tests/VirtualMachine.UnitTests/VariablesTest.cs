using Runtime;
using Semantics.Exceptions;
using TestsLibrary;
using VirtualMachine.Instructions;

namespace VirtualMachine.UnitTests;

public class VariablesTest
{
    [Theory]
    [MemberData(nameof(GetStoreAndLoadVariablesData))]
    public void Can_store_and_load_variable(
        IReadOnlyList<Instruction> program,
        string? input,
        int expectedExitCode
    )
    {
        FakeEnvironment environment = new();
        if (input is not null)
        {
            environment.AddInput(input);
        }

        DeaVM vm = new(environment, program);

        int result = vm.RunProgram();
        Assert.Equal(expectedExitCode, result);
    }

    public static TheoryData<IReadOnlyList<Instruction>, string?, int> GetStoreAndLoadVariablesData()
    {
        return new TheoryData<IReadOnlyList<Instruction>, string?, int>
        {
            // Объявление, присваивание, чтение переменной
            {
                [
                    new Instruction(InstructionCode.Push, new Value(10)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.DefineVar, new Value("x")),
                    new Instruction(InstructionCode.Push, new Value(5)),
                    new Instruction(InstructionCode.StoreVar, new Value("x")),
                    new Instruction(InstructionCode.LoadVar, new Value("x")),
                    new Instruction(InstructionCode.Halt),
                ],
                null,
                5
            },

            // Ввод в переменную и чтение
            {
                [
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.DefineVar, new Value("x")),
                    new Instruction(InstructionCode.InputVar, new Value("x")),
                    new Instruction(InstructionCode.LoadVar, new Value("x")),
                    new Instruction(InstructionCode.Halt),
                ],
                "0",
                0
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidVariableProgramsData))]
    public void Throws_for_invalid_variable_usage(
        IReadOnlyList<Instruction> program,
        string? input
    )
    {
        FakeEnvironment environment = new();
        if (input is not null)
        {
            environment.AddInput(input);
        }

        DeaVM vm = new(environment, program);
        Assert.Throws<RuntimeException>(() => vm.RunProgram());
    }

    public static TheoryData<IReadOnlyList<Instruction>, string?> GetInvalidVariableProgramsData()
    {
        return new TheoryData<IReadOnlyList<Instruction>, string?>
        {
            // Запрет присваивания в const
            {
                [
                    new Instruction(InstructionCode.Push, new Value(10)),
                    new Instruction(InstructionCode.Push, new Value(1)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.DefineVar, new Value("x")),
                    new Instruction(InstructionCode.Push, new Value(11)),
                    new Instruction(InstructionCode.StoreVar, new Value("x")),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ],
                null
            },

            // Чтение неинициализированной переменной
            {
                [
                    new Instruction(InstructionCode.Push, Value.Void),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.DefineVar, new Value("x")),
                    new Instruction(InstructionCode.LoadVar, new Value("x")),
                    new Instruction(InstructionCode.Halt),
                ],
                null
            },

            // Невалидный ввод для int
            {
                [
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.DefineVar, new Value("x")),
                    new Instruction(InstructionCode.InputVar, new Value("x")),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Halt),
                ],
                "abc"
            },

            // Повторное объявление 'x' в одном блоке
            {
                [
                    new Instruction(InstructionCode.Push, new Value(10)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.DefineVar, new Value("x")),

                    new Instruction(InstructionCode.Push, new Value(20)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.DefineVar, new Value("x")),

                    new Instruction(InstructionCode.Halt),
                ],
                null
            },
        };
    }
}