using Runtime;
using VirtualMachine.Builtins;
using VirtualMachine.Instructions;
using VirtualMachineCodegen;
using DeaParser = global::Parser.Parser;

namespace VirtualMachineCodegen.UnitTests;

public class DeaVmCodegenTests
{
    [Fact]
    public void Generates_push_and_print_for_each_literal_and_halt_for_return()
    {
        string code = """
            func int main() {
                print(1, 2.5, "dea");
                return 7;
            }
            """;

        Ast.ProgramNode program = new DeaParser(code).ParseProgram();
        DeaVmCodegen codegen = new();

        IReadOnlyList<Instruction> instructions = codegen.Generate(program);

        Assert.Equal(8, instructions.Count);
        Assert.Equal(InstructionCode.Push, instructions[0].Code);
        Assert.Equal(1, instructions[0].Operand.AsInt());
        Assert.Equal(InstructionCode.CallBuiltin, instructions[1].Code);
        Assert.Equal((int)BuiltinFunctionCode.Print, instructions[1].Operand.AsInt());

        Assert.Equal(InstructionCode.Push, instructions[2].Code);
        Assert.Equal(2.5, instructions[2].Operand.AsNum());
        Assert.Equal(InstructionCode.CallBuiltin, instructions[3].Code);

        Assert.Equal(InstructionCode.Push, instructions[4].Code);
        Assert.Equal("dea", instructions[4].Operand.AsString());
        Assert.Equal(InstructionCode.CallBuiltin, instructions[5].Code);

        Assert.Equal(InstructionCode.Push, instructions[6].Code);
        Assert.Equal(7, instructions[6].Operand.AsInt());
        Assert.Equal(InstructionCode.Halt, instructions[7].Code);
    }
}