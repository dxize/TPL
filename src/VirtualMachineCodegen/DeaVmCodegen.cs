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
    private readonly Dictionary<string, SymbolEntry> _symbols = new(StringComparer.Ordinal);

    public IReadOnlyList<Instruction> Generate(ProgramNode program)
    {
        _instructions.Clear();
        _symbols.Clear();
        program.Accept(this);
        return _instructions.ToArray();
    }

    public void Visit(ProgramNode p)
    {
        foreach (Declaration decl in p.GlobalDeclarations)
        {
            decl.Accept(this);
        }

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

    public void Visit(IdentifierExpression e)
    {
        _instructions.Add(new Instruction(InstructionCode.LoadVar, new Value(e.Name)));
    }

    public void Visit(UnaryExpression e)
    {
        e.Operand.Accept(this);
        if (e.OperatorKind == OperatorKind.Minus)
        {
            _instructions.Add(new Instruction(InstructionCode.Negate));
        }
    }

    public void Visit(BinaryExpression e)
    {
        e.Left.Accept(this);
        e.Right.Accept(this);
        _instructions.Add(new Instruction(MapBinaryInstruction(e.OperatorKind)));
    }

    public void Visit(CallExpression e)
    {
        foreach (Expression argument in e.Arguments)
        {
            argument.Accept(this);
        }

        if (string.Equals(e.Name, "min", StringComparison.Ordinal) || string.Equals(e.Name, "max", StringComparison.Ordinal))
        {
            _instructions.Add(new Instruction(InstructionCode.Push, new Value(e.Arguments.Count)));
        }

        _instructions.Add(new Instruction(InstructionCode.CallBuiltin, new Value((int)MapBuiltin(e.Name))));
    }

    public void Visit(VariableDeclarationExpression e)
    {
        if (_symbols.ContainsKey(e.Name))
        {
            throw new InvalidOperationException($"Identifier '{e.Name}' already declared.");
        }

        if (e.Initializer is null)
        {
            _instructions.Add(new Instruction(InstructionCode.Push, Value.Void));
        }
        else
        {
            e.Initializer.Accept(this);
        }

        _instructions.Add(new Instruction(InstructionCode.Push, new Value(0))); // mutable
        _instructions.Add(new Instruction(InstructionCode.Push, new Value(ToTypeTag(e.Type))));
        _instructions.Add(new Instruction(InstructionCode.DefineVar, new Value(e.Name)));
        _symbols[e.Name] = new SymbolEntry(e.Type, isConst: false);
    }

    public void Visit(ConstantDeclarationExpression e)
    {
        if (_symbols.ContainsKey(e.Name))
        {
            throw new InvalidOperationException($"Identifier '{e.Name}' already declared.");
        }

        e.Initializer.Accept(this);
        _instructions.Add(new Instruction(InstructionCode.Push, new Value(1))); // const
        _instructions.Add(new Instruction(InstructionCode.Push, new Value(ToTypeTag(e.Type))));
        _instructions.Add(new Instruction(InstructionCode.DefineVar, new Value(e.Name)));
        _symbols[e.Name] = new SymbolEntry(e.Type, isConst: true);
    }

    public void Visit(AssignmentExpression e)
    {
        if (_symbols.TryGetValue(e.Name, out SymbolEntry? symbol) && symbol.IsConst)
        {
            throw new InvalidOperationException($"Cannot assign to const '{e.Name}'.");
        }

        e.Value.Accept(this);
        _instructions.Add(new Instruction(InstructionCode.StoreVar, new Value(e.Name)));
    }

    public void Visit(InputExpression e)
    {
        _instructions.Add(new Instruction(InstructionCode.InputVar, new Value(e.VariableName)));
    }

    public void Visit(PrintExpression e)
    {
        foreach (Expression argument in e.Arguments)
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
            _ => throw new InvalidOperationException($"Unsupported data type '{literal.Type}'."),
        };
    }

    private static InstructionCode MapBinaryInstruction(OperatorKind operatorKind)
    {
        return operatorKind switch
        {
            OperatorKind.Plus => InstructionCode.Add,
            OperatorKind.Minus => InstructionCode.Subtract,
            OperatorKind.Multiply => InstructionCode.Multiply,
            OperatorKind.Divide => InstructionCode.Divide,
            OperatorKind.IntegerDivide => InstructionCode.IntegerDivide,
            OperatorKind.Modulo => InstructionCode.Modulo,
            OperatorKind.Power => InstructionCode.Power,
            _ => throw new InvalidOperationException($"Unsupported operator '{operatorKind}'."),
        };
    }

    private static int ToTypeTag(DataType type)
    {
        return type switch
        {
            DataType.Int => 0,
            DataType.Num => 1,
            DataType.String => 2,
            _ => throw new InvalidOperationException($"Unsupported type '{type}'."),
        };
    }

    private static BuiltinFunctionCode MapBuiltin(string name)
    {
        return name switch
        {
            "len" => BuiltinFunctionCode.Len,
            "substr" => BuiltinFunctionCode.Substr,
            "abs" => BuiltinFunctionCode.Abs,
            "min" => BuiltinFunctionCode.Min,
            "max" => BuiltinFunctionCode.Max,
            _ => throw new InvalidOperationException($"Unsupported function '{name}'."),
        };
    }

    private sealed class SymbolEntry
    {
        public SymbolEntry(DataType type, bool isConst)
        {
            Type = type;
            IsConst = isConst;
        }

        public DataType Type { get; }

        public bool IsConst { get; }
    }
}