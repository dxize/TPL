using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Runtime;

using VirtualMachine;
using VirtualMachine.Builtins;
using VirtualMachine.Instructions;

namespace VirtualMachineCodegen;

public sealed class DeaVmCodegen : IAstVisitor
{
    private readonly List<Instruction> _instructions = [];
    private readonly Dictionary<string, CompiledFunctionInfo> _functions = new(StringComparer.Ordinal);
    private FunctionDeclaration? _currentFunction;
    private readonly Stack<int> _breakStack = [];
    private readonly Stack<int> _continueStack = [];
    private int _currentBreakTarget = -1;
    private int _currentContinueTarget = -1;

    public CompiledProgram GenerateProgram(ProgramNode program)
    {
        _instructions.Clear();
        _functions.Clear();

        program.Accept(this);

        return new CompiledProgram(
            _instructions.ToArray(),
            new Dictionary<string, CompiledFunctionInfo>(_functions, StringComparer.Ordinal));
    }

    public void Visit(ProgramNode p)
    {
        foreach (Declaration decl in p.GlobalDeclarations)
        {
            decl.Accept(this);
        }

        int jumpToEntryIndex = Emit(InstructionCode.Jump, new Value(-1));

        foreach (FunctionDeclaration function in p.UserFunctions.Append(p.MainFunction))
        {
            _functions[function.Name] = new CompiledFunctionInfo(
                function.Name,
                _instructions.Count,
                function.Parameters.Select(x => x.Name).ToArray(),
                function.Parameters.Select(x => ToRuntimeType(x.Type)).ToArray(),
                ToRuntimeType(function.ReturnType));

            FunctionDeclaration? previousFunction = _currentFunction;
            _currentFunction = function;

            foreach (AstNode node in function.Body)
            {
                node.Accept(this);
            }

            if (_instructions.Count == 0 || _instructions[^1].Code != InstructionCode.Return)
            {
                if (!function.IsProcedure)
                {
                    _instructions.Add(new Instruction(InstructionCode.Push, Value.Void));
                }

                _instructions.Add(new Instruction(InstructionCode.Return));
            }

            _currentFunction = previousFunction;
        }

        int entryPoint = _instructions.Count;
        Patch(jumpToEntryIndex, InstructionCode.Jump, entryPoint);
        _instructions.Add(new Instruction(InstructionCode.CallUser, new Value("main")));
        _instructions.Add(new Instruction(InstructionCode.Halt));
    }

    public void Visit(FunctionDeclaration d)
    {
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
            return;
        }

        if (e.OperatorKind == OperatorKind.Not)
        {
            EmitConvertToBoolIfNeeded(e.Operand);
            _instructions.Add(new Instruction(InstructionCode.Not));
        }
    }

    public void Visit(BinaryExpression e)
    {
        if (e.OperatorKind == OperatorKind.And )
        {
            EmitLogicalAnd(e);
            return;
        }

        if (e.OperatorKind == OperatorKind.Or)
        {
            EmitLogicalOr(e);
            return;
        }

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

        if (e.IsBuiltin)
        {
            if (string.Equals(e.Name, "min", StringComparison.Ordinal) ||
                string.Equals(e.Name, "max", StringComparison.Ordinal))
            {
                _instructions.Add(new Instruction(InstructionCode.Push, new Value(e.Arguments.Count)));
            }

            _instructions.Add(new Instruction(InstructionCode.CallBuiltin, new Value((int)MapBuiltin(e.Name))));
            return;
        }

        _instructions.Add(new Instruction(InstructionCode.CallUser, new Value(e.Name)));
    }

    public void Visit(ProcedureCallStatement s)
    {
        foreach (Expression argument in s.Arguments)
        {
            argument.Accept(this);
        }

        _instructions.Add(new Instruction(InstructionCode.CallUser, new Value(s.Name)));
    }

    public void Visit(VariableDeclarationExpression e)
    {
        if (e.Initializer is null)
        {
            _instructions.Add(new Instruction(InstructionCode.Push, Value.Void));
        }
        else
        {
            e.Initializer.Accept(this);
        }

        _instructions.Add(new Instruction(InstructionCode.Push, new Value(0)));
        _instructions.Add(new Instruction(InstructionCode.Push, new Value(ToTypeTag(e.Type))));
        _instructions.Add(new Instruction(InstructionCode.DefineVar, new Value(e.Name)));
    }

    public void Visit(ConstantDeclarationExpression e)
    {
        e.Initializer.Accept(this);
        _instructions.Add(new Instruction(InstructionCode.Push, new Value(1)));
        _instructions.Add(new Instruction(InstructionCode.Push, new Value(ToTypeTag(e.Type))));
        _instructions.Add(new Instruction(InstructionCode.DefineVar, new Value(e.Name)));
    }

    public void Visit(AssignmentExpression e)
    {
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
            _instructions.Add(new Instruction(InstructionCode.CallBuiltin, new Value((int)BuiltinFunctionCode.Print)));
        }
    }

    public void Visit(ReturnExpression e)
    {
        if (e.Value is not null)
        {
            e.Value.Accept(this);
        }

        _instructions.Add(new Instruction(InstructionCode.Return));
    }

    public void Visit(IfStatement s)
    {
        s.Condition.Accept(this);
        EmitConvertToBoolIfNeeded(s.Condition);

        int jumpToElseIndex = Emit(InstructionCode.JumpIfFalse, new Value(-1));

        foreach (AstNode node in s.ThenBody)
        {
            node.Accept(this);
        }

        if (s.ElseBody is null)
        {
            Patch(jumpToElseIndex, InstructionCode.JumpIfFalse, _instructions.Count);
            return;
        }

        int jumpToEndIndex = Emit(InstructionCode.Jump, new Value(-1));
        Patch(jumpToElseIndex, InstructionCode.JumpIfFalse, _instructions.Count);

        foreach (AstNode node in s.ElseBody)
        {
            node.Accept(this);
        }

        Patch(jumpToEndIndex, InstructionCode.Jump, _instructions.Count);
    }

    public void Visit(WhileStatement s)
    {
        int loopStartIndex = _instructions.Count;

        int previousBreakTarget = _currentBreakTarget;
        int previousContinueTarget = _currentContinueTarget;
        _currentBreakTarget = -1;
        _currentContinueTarget = loopStartIndex;

        s.Condition.Accept(this);
        EmitConvertToBoolIfNeeded(s.Condition);

        int jumpToEndIndex = Emit(InstructionCode.JumpIfFalse, new Value(-1));

        foreach (AstNode node in s.Body)
        {
            node.Accept(this);
        }

        while (_continueStack.Count > 0)
        {
            int continuePos = _continueStack.Pop();
            Patch(continuePos, InstructionCode.Jump, _instructions.Count);
        }

        Emit(InstructionCode.Jump, new Value(loopStartIndex));
        Patch(jumpToEndIndex, InstructionCode.JumpIfFalse, _instructions.Count);

        while (_breakStack.Count > 0)
        {
            int breakPos = _breakStack.Pop();
            Patch(breakPos, InstructionCode.Jump, _instructions.Count);
        }

        _currentBreakTarget = previousBreakTarget;
        _currentContinueTarget = previousContinueTarget;
    }

    public void Visit(ForStatement s)
    {
        s.Start.Accept(this);
        _instructions.Add(new Instruction(InstructionCode.StoreVar, new Value(s.VariableName)));

        int loopStartIndex = _instructions.Count;

        int previousBreakTarget = _currentBreakTarget;
        int previousContinueTarget = _currentContinueTarget;
        _currentBreakTarget = -1;
        _currentContinueTarget = loopStartIndex;

        _instructions.Add(new Instruction(InstructionCode.LoadVar, new Value(s.VariableName)));
        s.End.Accept(this);

        _instructions.Add(new Instruction(s.Descending ? InstructionCode.GreaterOrEqual : InstructionCode.LessOrEqual));

        int jumpToEndIndex = Emit(InstructionCode.JumpIfFalse, new Value(-1));

        foreach (AstNode node in s.Body)
        {
            node.Accept(this);
        }

        while (_continueStack.Count > 0)
        {
            int continuePos = _continueStack.Pop();
            Patch(continuePos, InstructionCode.Jump, _instructions.Count);
        }

        _instructions.Add(new Instruction(InstructionCode.LoadVar, new Value(s.VariableName)));
        _instructions.Add(new Instruction(InstructionCode.Push, new Value(s.Descending ? -1 : 1)));
        _instructions.Add(new Instruction(InstructionCode.Add));
        _instructions.Add(new Instruction(InstructionCode.StoreVar, new Value(s.VariableName)));

        Emit(InstructionCode.Jump, new Value(loopStartIndex));
        Patch(jumpToEndIndex, InstructionCode.JumpIfFalse, _instructions.Count);

        while (_breakStack.Count > 0)
        {
            int breakPos = _breakStack.Pop();
            Patch(breakPos, InstructionCode.Jump, _instructions.Count);
        }

        _currentBreakTarget = previousBreakTarget;
        _currentContinueTarget = previousContinueTarget;
    }

    public void Visit(BreakStatement s)
    {
        int breakIndex = Emit(InstructionCode.Jump, new Value(-1));
        _breakStack.Push(breakIndex);
    }

    public void Visit(ContinueStatement s)
    {
        int continueIndex = Emit(InstructionCode.Jump, new Value(-1));
        _continueStack.Push(continueIndex);
    }

    private void EmitLogicalAnd(BinaryExpression e)
    {
        e.Left.Accept(this);
        EmitConvertToBoolIfNeeded(e.Left);
        int jumpFalseFromLeft = Emit(InstructionCode.JumpIfFalse, new Value(-1));

        e.Right.Accept(this);
        EmitConvertToBoolIfNeeded(e.Right);
        int jumpFalseFromRight = Emit(InstructionCode.JumpIfFalse, new Value(-1));

        _instructions.Add(new Instruction(InstructionCode.Push, new Value(true)));
        int jumpEnd = Emit(InstructionCode.Jump, new Value(-1));

        int falseLabel = _instructions.Count;
        Patch(jumpFalseFromLeft, InstructionCode.JumpIfFalse, falseLabel);
        Patch(jumpFalseFromRight, InstructionCode.JumpIfFalse, falseLabel);
        _instructions.Add(new Instruction(InstructionCode.Push, new Value(false)));

        Patch(jumpEnd, InstructionCode.Jump, _instructions.Count);
    }

    private void EmitLogicalOr(BinaryExpression e)
    {
        e.Left.Accept(this);
        EmitConvertToBoolIfNeeded(e.Left);
        int jumpToEvaluateRight = Emit(InstructionCode.JumpIfFalse, new Value(-1));

        _instructions.Add(new Instruction(InstructionCode.Push, new Value(true)));
        int jumpEnd = Emit(InstructionCode.Jump, new Value(-1));

        int rightLabel = _instructions.Count;
        Patch(jumpToEvaluateRight, InstructionCode.JumpIfFalse, rightLabel);

        e.Right.Accept(this);
        EmitConvertToBoolIfNeeded(e.Right);

        Patch(jumpEnd, InstructionCode.Jump, _instructions.Count);
    }

    private void EmitConvertToBoolIfNeeded(Expression expression)
    {
        if (expression.ResultType != DataType.Bool)
        {
            _instructions.Add(new Instruction(InstructionCode.ToBool));
        }
    }

    private static Value MapLiteral(LiteralExpression literal)
    {
        return literal.Type switch
        {
            DataType.Int => new Value((int)literal.Value),
            DataType.Num => new Value((double)literal.Value),
            DataType.String => new Value((string)literal.Value),
            DataType.Bool => new Value((bool)literal.Value),
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
            OperatorKind.Equal => InstructionCode.Equal,
            OperatorKind.NotEqual => InstructionCode.NotEqual,
            OperatorKind.Less => InstructionCode.Less,
            OperatorKind.LessOrEqual => InstructionCode.LessOrEqual,
            OperatorKind.Greater => InstructionCode.Greater,
            OperatorKind.GreaterOrEqual => InstructionCode.GreaterOrEqual,
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
            DataType.Bool => 3,
            _ => throw new InvalidOperationException($"Unsupported type '{type}'."),
        };
    }

    private static Runtime.ValueType ToRuntimeType(DataType type)
    {
        return type switch
        {
            DataType.Int => Runtime.ValueType.Int,
            DataType.Num => Runtime.ValueType.Num,
            DataType.String => Runtime.ValueType.String,
            DataType.Bool => Runtime.ValueType.Bool,
            DataType.Void => Runtime.ValueType.Void,
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

    private int Emit(InstructionCode code, Value operand)
    {
        _instructions.Add(new Instruction(code, operand));
        return _instructions.Count - 1;
    }

    private void Patch(int index, InstructionCode code, int target)
    {
        _instructions[index] = new Instruction(code, new Value(target));
    }
}