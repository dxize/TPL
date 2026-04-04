using Runtime;
using TestsLibrary;
using VirtualMachine.Instructions;

namespace VirtualMachine.UnitTests;

public class EvaluationTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateExpressionData))]
    public void Can_evaluate_expression(List<Instruction> instructions, Value expected)
    {
        FakeEnvironment environment = new();
        DeaVM vm = new(environment, instructions);

        Value result = new Value(vm.RunProgram());

        // TODO: переопределить операцию сравнения двух Value
        Assert.Equal(expected.AsInt(), result.AsInt());
        Assert.Equal(string.Empty, environment.Output);
    }

    public static TheoryData<List<Instruction>, Value> GetEvaluateExpressionData()
    {
        return new TheoryData<List<Instruction>, Value>
        {
            // Возврат одного значения со стека
            {
                [
                    new Instruction(InstructionCode.Push, new Value(67)),
                    new Instruction(InstructionCode.Halt)
                ],
                new Value(67)
            },

            // Сложение и вычитание: (20 + 50) - 3 = 67
            {
                [
                    new Instruction(InstructionCode.Push, new Value(20)),
                    new Instruction(InstructionCode.Push, new Value(50)),
                    new Instruction(InstructionCode.Add),
                    new Instruction(InstructionCode.Push, new Value(3)),
                    new Instruction(InstructionCode.Subtract),
                    new Instruction(InstructionCode.Halt)
                ],
                new Value(67)
            },

            // Умножение и деление: (20 * 50) / 5 = 200
            {
                [
                    new Instruction(InstructionCode.Push, new Value(20)),
                    new Instruction(InstructionCode.Push, new Value(50)),
                    new Instruction(InstructionCode.Multiply),
                    new Instruction(InstructionCode.Push, new Value(5)),
                    new Instruction(InstructionCode.Divide),
                    new Instruction(InstructionCode.Halt)
                ],
                new Value(200)
            },

            // Целочисленное деление и остаток: 17 // 4 + 17 % 4 = 5
            {
                [
                    new Instruction(InstructionCode.Push, new Value(17)),
                    new Instruction(InstructionCode.Push, new Value(4)),
                    new Instruction(InstructionCode.IntegerDivide),
                    new Instruction(InstructionCode.Push, new Value(17)),
                    new Instruction(InstructionCode.Push, new Value(4)),
                    new Instruction(InstructionCode.Modulo),
                    new Instruction(InstructionCode.Add),
                    new Instruction(InstructionCode.Halt)
                ],
                new Value(5)
            },

            // Возведение в степень: 2 ^ 10 = 1024
            {
                [
                    new Instruction(InstructionCode.Push, new Value(2)),
                    new Instruction(InstructionCode.Push, new Value(10)),
                    new Instruction(InstructionCode.Power),
                    new Instruction(InstructionCode.Halt)
                ],
                new Value(1024)
            },

            // Унарный минус: -1024
            {
                [
                    new Instruction(InstructionCode.Push, new Value(1024)),
                    new Instruction(InstructionCode.Negate),
                    new Instruction(InstructionCode.Halt)
                ],
                new Value(-1024)
            },
        };
    }
}