using Ast;
using Ast.Declarations;
using Ast.Expressions;
using Runtime;
using VirtualMachine.Builtins;
using VirtualMachine.Instructions;

namespace VirtualMachineCodegen;

public sealed class DeaVmCodegen : IAstVisitor
{
    private readonly List<Instruction> _instructions = [];

    public IReadOnlyList<Instruction> Generate(ProgramNode program)
    {
        _instructions.Clear();
        program.Accept(this);
        return _instructions.ToArray();
    }

    public void Visit(ProgramNode p)
    {
        p.MainFunction.Accept(this);
    }

    public void Visit(FunctionDeclaration d)
    {
        foreach (AstNode node in d.Body)
        {
            node.Accept(this);
        }
    }

    public void Visit(LiteralExpression e)
    {
        _instructions.Add(new Instruction(InstructionCode.Push, MapLiteral(e)));
    }

    public void Visit(PrintExpression e)
    {
        foreach (LiteralExpression argument in e.Arguments)
        {
            argument.Accept(this);
            _instructions.Add(new Instruction(
                InstructionCode.CallBuiltin,
                new Value((int)BuiltinFunctionCode.Print)));
        }
    }

    public void Visit(ReturnExpression e)
    {
        e.Value.Accept(this);
        _instructions.Add(new Instruction(InstructionCode.Halt));
    }

    private static Value MapLiteral(LiteralExpression literal)
    {
        return literal.Type switch
        {
            DataType.Int => new Value((int)literal.Value),
            DataType.Num => new Value((double)literal.Value),
            DataType.String => new Value((string)literal.Value),
            _ => throw new InvalidOperationException($"Unsupported data type '{literal.Type}'.")
        };
    }
}
