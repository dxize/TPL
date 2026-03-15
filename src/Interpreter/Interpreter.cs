using Semantics;
using VirtualMachine;
using VirtualMachineCodegen;

using DeaParser = global::Parser.Parser;

namespace Interpreter;

public sealed class Interpreter
{
    private readonly IEnvironment _environment;

    public Interpreter(IEnvironment environment)
    {
        _environment = environment;
    }

    public int Execute(string sourceCode)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
        {
            throw new ArgumentException("Source code cannot be null or empty.", nameof(sourceCode));
        }

        DeaParser parser = new(sourceCode);
        Ast.ProgramNode program = parser.ParseProgram();

        SemanticsChecker checker = new();
        checker.Check(program);

        DeaVmCodegen codegen = new();
        IReadOnlyList<VirtualMachine.Instructions.Instruction> instructions = codegen.Generate(program);

        DeaVM vm = new(_environment, instructions);
        return vm.RunProgram();
    }
}