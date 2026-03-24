using Runtime;
using TestsLibrary;
using VirtualMachine.Instructions;

namespace VirtualMachine.UnitTests;

public class EvaluationTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateExpressionData))]
    public void Can_evaluate_expression(IReadOnlyList<Instruction> instructions, int expected)
    {
        FakeEnvironment environment = new();
        DeaVM vm = new(environment, instructions);

        int result = vm.RunProgram();
        Assert.Equal(expected, result);
        Assert.Equal(string.Empty, environment.Output);
    }

    public static TheoryData<IReadOnlyList<Instruction>, int> GetEvaluateExpressionData()
    {
        return new TheoryData<IReadOnlyList<Instruction>, int>
        {
            // (20 + 50) - 3 = 67
            {
                [
                    new Instruction(InstructionCode.Push, new Value(20)),
                    new Instruction(InstructionCode.Push, new Value(50)),
                    new Instruction(InstructionCode.Add),
                    new Instruction(InstructionCode.Push, new Value(3)),
                    new Instruction(InstructionCode.Subtract),
                    new Instruction(InstructionCode.Halt),
                ],
                67
            },
            // 17 // 4 = 4
            {
                [
                    new Instruction(InstructionCode.Push, new Value(17)),
                    new Instruction(InstructionCode.Push, new Value(4)),
                    new Instruction(InstructionCode.IntegerDivide),
                    new Instruction(InstructionCode.Halt),
                ],
                4
            },
            // 17 % 4 = 1
            {
                [
                    new Instruction(InstructionCode.Push, new Value(17)),
                    new Instruction(InstructionCode.Push, new Value(4)),
                    new Instruction(InstructionCode.Modulo),
                    new Instruction(InstructionCode.Halt),
                ],
                1
            },
            // 2 ^ (3 ^ 2) = 512 (порядок зашит в последовательности команд)
            {
                [
                    new Instruction(InstructionCode.Push, new Value(2)),
                    new Instruction(InstructionCode.Push, new Value(3)),
                    new Instruction(InstructionCode.Push, new Value(2)),
                    new Instruction(InstructionCode.Power),
                    new Instruction(InstructionCode.Power),
                    new Instruction(InstructionCode.Halt),
                ],
                512
            },
            // -1024
            {
                [
                    new Instruction(InstructionCode.Push, new Value(1024)),
                    new Instruction(InstructionCode.Negate),
                    new Instruction(InstructionCode.Halt),
                ],
                -1024
            },
        };
    }

    [Theory]
    [MemberData(nameof(GetInvalidEvaluateExpressionData))]
    public void Throws_for_invalid_numeric_operations(IReadOnlyList<Instruction> instructions)
    {
        FakeEnvironment environment = new();
        DeaVM vm = new(environment, instructions);

        Assert.Throws<InvalidOperationException>(() => vm.RunProgram());
    }

    public static TheoryData<IReadOnlyList<Instruction>> GetInvalidEvaluateExpressionData()
    {
        return new TheoryData<IReadOnlyList<Instruction>>
        {
            // Деление на ноль
            {
                [
                    new Instruction(InstructionCode.Push, new Value(1)),
                    new Instruction(InstructionCode.Push, new Value(0)),
                    new Instruction(InstructionCode.Divide),
                    new Instruction(InstructionCode.Halt),
                ]
            },
            // Нельзя складывать string и int
            {
                [
                    new Instruction(InstructionCode.Push, new Value("dea")),
                    new Instruction(InstructionCode.Push, new Value(1)),
                    new Instruction(InstructionCode.Add),
                    new Instruction(InstructionCode.Halt),
                ]
            },
        };
    }
}
