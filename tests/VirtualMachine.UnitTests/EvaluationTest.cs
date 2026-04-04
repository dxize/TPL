using Runtime;
using TestsLibrary;

using VirtualMachine.Builtins;
using VirtualMachine.Instructions;

namespace VirtualMachine.UnitTests;

public class EvaluationTest
{
    [Theory]
    [MemberData(nameof(GetEvaluateExpressionData))]
    public void Can_evaluate_expression(List<Instruction> instructions, int expected)
    {
        FakeEnvironment environment = new();
        DeaVM vm = new(environment, instructions);

        int result = vm.RunProgram();

        Assert.Equal(expected, result);
        Assert.Equal(string.Empty, environment.Output);
    }

    public static TheoryData<List<Instruction>, int> GetEvaluateExpressionData()
    {
        return new TheoryData<List<Instruction>, int>
        {
            // Возврат одного значения со стека
            {
                [
                    new Instruction(InstructionCode.Push, new Value(67)),
                    new Instruction(InstructionCode.Halt)
                ],
                67
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
                67
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
                5
            },

            // Возведение в степень: 2 ^ 10 = 1024
            {
                [
                    new Instruction(InstructionCode.Push, new Value(2)),
                    new Instruction(InstructionCode.Push, new Value(10)),
                    new Instruction(InstructionCode.Power),
                    new Instruction(InstructionCode.Halt)
                ],
                1024
            },

            // Унарный минус: -1024
            {
                [
                    new Instruction(InstructionCode.Push, new Value(1024)),
                    new Instruction(InstructionCode.Negate),
                    new Instruction(InstructionCode.Halt)
                ],
                -1024
            },
        };
    }

    [Fact]
    public void Can_divide_with_floating_point_result()
    {
        FakeEnvironment environment = new();

        // (20 * 50) / 5 = 200.0
        List<Instruction> program = [
            new(InstructionCode.Push, new Value(20)),
            new(InstructionCode.Push, new Value(50)),
            new(InstructionCode.Multiply),
            new(InstructionCode.Push, new Value(5)),
            new(InstructionCode.Divide), // Результат: Num
            new(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)), // print выведет как "200"
            new(InstructionCode.Push, new Value(0)),
            new(InstructionCode.Halt)
        ];

        DeaVM vm = new(environment, program);
        int exitCode = vm.RunProgram();

        Assert.Equal(0, exitCode);
        Assert.Equal("200", environment.Output);
    }
}