using Ast;
using Ast.Declarations;

using VirtualMachine;
using VirtualMachine.Builtins;
using VirtualMachine.Instructions;

using DeaParser = global::Parser.Parser;

namespace VirtualMachineCodegen.UnitTests;

public class DeaVmCodegenTests
{
    [Fact]
    public void Generates_push_and_print_for_each_literal_and_return_for_main_program()
    {
        string code = """
            func int main() {
                print(1, 2.5, "dea");
                return 7;
            }
            """;

        Ast.ProgramNode program = new DeaParser(code).ParseProgram();
        DeaVmCodegen codegen = new();

        IReadOnlyList<Instruction> instructions = codegen.GenerateProgram(program).Instructions;

        Assert.Equal(11, instructions.Count);

        Assert.Equal(InstructionCode.Jump, instructions[0].Code);

        Assert.Equal(InstructionCode.Push, instructions[1].Code);
        Assert.Equal(1, instructions[1].Operand.AsInt());
        Assert.Equal(InstructionCode.CallBuiltin, instructions[2].Code);
        Assert.Equal((int)BuiltinFunctionCode.Print, instructions[2].Operand.AsInt());

        Assert.Equal(InstructionCode.Push, instructions[3].Code);
        Assert.Equal(2.5, instructions[3].Operand.AsNum());
        Assert.Equal(InstructionCode.CallBuiltin, instructions[4].Code);
        Assert.Equal((int)BuiltinFunctionCode.Print, instructions[4].Operand.AsInt());

        Assert.Equal(InstructionCode.Push, instructions[5].Code);
        Assert.Equal("dea", instructions[5].Operand.AsString());
        Assert.Equal(InstructionCode.CallBuiltin, instructions[6].Code);
        Assert.Equal((int)BuiltinFunctionCode.Print, instructions[6].Operand.AsInt());

        Assert.Equal(InstructionCode.Push, instructions[7].Code);
        Assert.Equal(7, instructions[7].Operand.AsInt());
        Assert.Equal(InstructionCode.Return, instructions[8].Code);

        Assert.Equal(InstructionCode.CallUser, instructions[9].Code);
        Assert.Equal("main", instructions[9].Operand.AsString());
        Assert.Equal(InstructionCode.Halt, instructions[10].Code);
    }

    [Fact]
    public void Generates_code_for_valid_program()
    {
        string code = """
            func int main() {
                const int x = 10;
                print(x);
                return x;
            }
            """;

        Ast.ProgramNode program = new DeaParser(code).ParseProgram();
        DeaVmCodegen codegen = new();

        IReadOnlyList<Instruction> instructions = codegen.GenerateProgram(program).Instructions;

        Assert.NotEmpty(instructions);
    }

    [Fact]
    public void Codegen_AddsPushVoid_ForFuncWithoutReturn()
    {
        DeaVmCodegen codegen = new();

        FunctionDeclaration funcWithoutReturn = new(
            DataType.Int,
            "noReturnFunc",
            new List<ParameterDeclaration>(),
            new List<AstNode>());

        ProgramNode program = new(
            new List<Declaration>(),
            new List<FunctionDeclaration> { funcWithoutReturn },
            new FunctionDeclaration(DataType.Int, "main", new List<ParameterDeclaration>(), new List<AstNode>()));

        CompiledProgram compiled = codegen.GenerateProgram(program);

        Assert.Contains(compiled.Instructions, i => i.Code == InstructionCode.Push && i.Operand.IsVoid());
    }
}