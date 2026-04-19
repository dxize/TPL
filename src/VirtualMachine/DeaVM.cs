using Runtime;

using Semantics.Exceptions;

using VirtualMachine.Builtins;
using VirtualMachine.Instructions;

namespace VirtualMachine;

public sealed class DeaVM
{
    private readonly IReadOnlyList<Instruction> _instructions;
    private readonly IReadOnlyDictionary<string, CompiledFunctionInfo> _functions;
    private readonly BuiltinFunctions _builtins;
    private readonly IEnvironment _environment;
    private readonly Stack<Value> _evaluationStack;
    private readonly Dictionary<string, VariableEntry> _globals;
    private readonly Stack<CallFrame> _callFrames;
    private int _instructionPointer;

    /// <summary>
    /// Старый конструктор для тестов.
    /// </summary>
    public DeaVM(IEnvironment environment, IReadOnlyList<Instruction> instructions)
        : this(
            environment,
            new CompiledProgram(
                instructions,
                new Dictionary<string, CompiledFunctionInfo>(StringComparer.Ordinal)))
    {
    }

    public DeaVM(IEnvironment environment, CompiledProgram program)
    {
        _environment = environment;
        _instructions = program.Instructions;
        _functions = program.Functions;
        _builtins = new BuiltinFunctions(environment);
        _evaluationStack = [];
        _globals = new Dictionary<string, VariableEntry>(StringComparer.Ordinal);
        _callFrames = [];
        _instructionPointer = 0;
    }

    public int RunProgram()
    {
        while (true)
        {
            Instruction instruction = _instructions[_instructionPointer++];

            switch (instruction.Code)
            {
                case InstructionCode.Push:
                    _evaluationStack.Push(instruction.Operand);
                    break;

                case InstructionCode.DefineVar:
                    DefineVariable(instruction.Operand.AsString());
                    break;

                case InstructionCode.LoadVar:
                    _evaluationStack.Push(LoadVariable(instruction.Operand.AsString()));
                    break;

                case InstructionCode.StoreVar:
                    StoreVariable(instruction.Operand.AsString());
                    break;

                case InstructionCode.InputVar:
                    InputVariable(instruction.Operand.AsString());
                    break;

                case InstructionCode.Add:
                    ExecuteAdd();
                    break;

                case InstructionCode.Subtract:
                    ExecuteNumericBinary((a, b) => a - b, (a, b) => a - b);
                    break;

                case InstructionCode.Multiply:
                    ExecuteNumericBinary((a, b) => a * b, (a, b) => a * b);
                    break;

                case InstructionCode.Divide:
                    ExecuteDivide();
                    break;

                case InstructionCode.IntegerDivide:
                    ExecuteIntegerDivide();
                    break;

                case InstructionCode.Modulo:
                    ExecuteModulo();
                    break;

                case InstructionCode.Power:
                    ExecutePower();
                    break;

                case InstructionCode.Negate:
                    ExecuteNegate();
                    break;

                case InstructionCode.ToBool:
                    _evaluationStack.Push(ConvertToBool(_evaluationStack.Pop()));
                    break;

                case InstructionCode.Not:
                    _evaluationStack.Push(new Value(!_evaluationStack.Pop().AsBool()));
                    break;

                case InstructionCode.Equal:
                    ExecuteEquality(isEqual: true);
                    break;

                case InstructionCode.NotEqual:
                    ExecuteEquality(isEqual: false);
                    break;

                case InstructionCode.Less:
                    ExecuteComparison((a, b) => a < b);
                    break;

                case InstructionCode.LessOrEqual:
                    ExecuteComparison((a, b) => a <= b);
                    break;

                case InstructionCode.Greater:
                    ExecuteComparison((a, b) => a > b);
                    break;

                case InstructionCode.GreaterOrEqual:
                    ExecuteComparison((a, b) => a >= b);
                    break;

                case InstructionCode.CallBuiltin:
                    ExecuteBuiltin(instruction.Operand);
                    break;

                case InstructionCode.CallUser:
                    CallUserFunction(instruction.Operand.AsString());
                    break;

                case InstructionCode.Return:
                    ReturnFromFunction();
                    break;

                case InstructionCode.Jump:
                    _instructionPointer = instruction.Operand.AsInt();
                    break;

                case InstructionCode.JumpIfFalse:
                    if (!_evaluationStack.Pop().AsBool())
                    {
                        _instructionPointer = instruction.Operand.AsInt();
                    }

                    break;

                case InstructionCode.Halt:
                    return _evaluationStack.Pop().AsInt();

                default:
                    throw new RuntimeException($"Unsupported instruction '{instruction.Code}'.");
            }
        }
    }

    private void ExecuteBuiltin(Value operand)
    {
        BuiltinFunctionCode builtin = (BuiltinFunctionCode)operand.AsInt();

        switch (builtin)
        {
            case BuiltinFunctionCode.Print:
                _builtins.Print(_evaluationStack.Pop());
                break;

            case BuiltinFunctionCode.Len:
                _evaluationStack.Push(_builtins.Len(_evaluationStack.Pop()));
                break;

            case BuiltinFunctionCode.Substr:
                {
                    Value length = _evaluationStack.Pop();
                    Value start = _evaluationStack.Pop();
                    Value source = _evaluationStack.Pop();
                    _evaluationStack.Push(_builtins.Substr(source, start, length));
                    break;
                }

            case BuiltinFunctionCode.Abs:
                _evaluationStack.Push(_builtins.Abs(_evaluationStack.Pop()));
                break;

            case BuiltinFunctionCode.Min:
                ExecuteMinOrMax(isMin: true);
                break;

            case BuiltinFunctionCode.Max:
                ExecuteMinOrMax(isMin: false);
                break;

            default:
                throw new RuntimeException($"Unsupported builtin '{builtin}'.");
        }
    }

    private void DefineVariable(string name)
    {
        Value typeTag = _evaluationStack.Pop();
        Value isConstTag = _evaluationStack.Pop();
        Value value = _evaluationStack.Pop();

        bool hasInitializer = !value.IsVoid();
        Runtime.ValueType declaredType = typeTag.AsInt() switch
        {
            0 => Runtime.ValueType.Int,
            1 => Runtime.ValueType.Num,
            2 => Runtime.ValueType.String,
            3 => Runtime.ValueType.Bool,
            _ => throw new RuntimeException("Unsupported variable type tag."),
        };

        Dictionary<string, VariableEntry> target = GetCurrentVariables();

        if (target.ContainsKey(name))
        {
            throw new RuntimeException($"Identifier '{name}' already declared.");
        }

        target[name] = new VariableEntry(
            declaredType,
            isConstTag.AsInt() == 1,
            hasInitializer,
            hasInitializer ? value : Value.Void);
    }

    private Value LoadVariable(string name)
    {
        VariableEntry variable = GetVariable(name);

        if (!variable.IsInitialized)
        {
            throw new RuntimeException($"Variable '{name}' is not initialized.");
        }

        return variable.Value;
    }

    private void StoreVariable(string name)
    {
        Value value = _evaluationStack.Pop();
        VariableEntry variable = GetVariable(name);

        if (variable.IsConst)
        {
            throw new RuntimeException($"Cannot assign to constant '{name}'.");
        }

        variable.SetValue(value);
    }

    private void InputVariable(string name)
    {
        VariableEntry variable = GetVariable(name);

        if (variable.IsConst)
        {
            throw new RuntimeException($"Cannot read input into constant '{name}'.");
        }

        string text = _environment.ReadLine();

        Value value = variable.Type switch
        {
            _ when variable.Type == Runtime.ValueType.Int =>
                int.TryParse(text, out int intValue)
                    ? new Value(intValue)
                    : throw new RuntimeException($"Input value '{text}' is not int."),

            _ when variable.Type == Runtime.ValueType.Num =>
                double.TryParse(
                    text,
                    System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out double numValue)
                    ? new Value(numValue)
                    : throw new RuntimeException($"Input value '{text}' is not num."),

            _ when variable.Type == Runtime.ValueType.String =>
                new Value(text),

            _ when variable.Type == Runtime.ValueType.Bool =>
                bool.TryParse(text, out bool boolValue)
                    ? new Value(boolValue)
                    : throw new RuntimeException($"Input value '{text}' is not bool."),

            _ => throw new RuntimeException("Unsupported variable type for input."),
        };

        variable.SetValue(value);
    }

    private Dictionary<string, VariableEntry> GetCurrentVariables()
    {
        return _callFrames.Count > 0 ? _callFrames.Peek().Variables : _globals;
    }

    private VariableEntry GetVariable(string name)
    {
        if (_callFrames.Count > 0 && _callFrames.Peek().Variables.TryGetValue(name, out VariableEntry? localValue))
        {
            return localValue;
        }

        if (_globals.TryGetValue(name, out VariableEntry? globalValue))
        {
            return globalValue;
        }

        throw new RuntimeException($"Identifier '{name}' is not declared.");
    }

    private void ExecuteAdd()
    {
        Value right = _evaluationStack.Pop();
        Value left = _evaluationStack.Pop();

        if (left.Type == Runtime.ValueType.String && right.Type == Runtime.ValueType.String)
        {
            _evaluationStack.Push(new Value(left.AsString() + right.AsString()));
            return;
        }

        ExecuteNumericBinaryCore(left, right, (a, b) => a + b, (a, b) => a + b);
    }

    private void ExecuteNumericBinary(Func<int, int, int> intOperation, Func<double, double, double> numOperation)
    {
        Value right = _evaluationStack.Pop();
        Value left = _evaluationStack.Pop();
        ExecuteNumericBinaryCore(left, right, intOperation, numOperation);
    }

    private void ExecuteNumericBinaryCore(Value left, Value right, Func<int, int, int> intOperation, Func<double, double, double> numOperation)
    {
        if (left.Type == Runtime.ValueType.Int && right.Type == Runtime.ValueType.Int)
        {
            _evaluationStack.Push(new Value(intOperation(left.AsInt(), right.AsInt())));
            return;
        }

        _evaluationStack.Push(new Value(numOperation(left.AsNum(), right.AsNum())));
    }

    private void ExecuteDivide()
    {
        Value right = _evaluationStack.Pop();
        Value left = _evaluationStack.Pop();

        double divisor = right.AsNum();
        if (Math.Abs(divisor) < double.Epsilon)
        {
            throw new RuntimeException("Division by zero.");
        }

        _evaluationStack.Push(new Value(left.AsNum() / divisor));
    }

    private void ExecuteIntegerDivide()
    {
        Value right = _evaluationStack.Pop();
        Value left = _evaluationStack.Pop();

        int divisor = right.AsInt();
        if (divisor == 0)
        {
            throw new RuntimeException("Division by zero.");
        }

        _evaluationStack.Push(new Value(left.AsInt() / divisor));
    }

    private void ExecuteModulo()
    {
        Value right = _evaluationStack.Pop();
        Value left = _evaluationStack.Pop();

        int divisor = right.AsInt();
        if (divisor == 0)
        {
            throw new RuntimeException("Division by zero.");
        }

        _evaluationStack.Push(new Value(left.AsInt() % divisor));
    }

    private void ExecutePower()
    {
        Value right = _evaluationStack.Pop();
        Value left = _evaluationStack.Pop();

        if (left.Type == Runtime.ValueType.Int && right.Type == Runtime.ValueType.Int)
        {
            _evaluationStack.Push(new Value((int)Math.Pow(left.AsInt(), right.AsInt())));
            return;
        }

        _evaluationStack.Push(new Value(Math.Pow(left.AsNum(), right.AsNum())));
    }

    private void ExecuteNegate()
    {
        Value value = _evaluationStack.Pop();

        if (value.Type == Runtime.ValueType.Int)
        {
            _evaluationStack.Push(new Value(-value.AsInt()));
            return;
        }

        _evaluationStack.Push(new Value(-value.AsNum()));
    }

    private void ExecuteEquality(bool isEqual)
    {
        Value right = _evaluationStack.Pop();
        Value left = _evaluationStack.Pop();

        bool result;

        if ((left.Type == Runtime.ValueType.Int || left.Type == Runtime.ValueType.Num) &&
            (right.Type == Runtime.ValueType.Int || right.Type == Runtime.ValueType.Num))
        {
            result = Math.Abs(left.AsNum() - right.AsNum()) < double.Epsilon;
        }
        else if (left.Type == Runtime.ValueType.String && right.Type == Runtime.ValueType.String)
        {
            result = string.Equals(left.AsString(), right.AsString(), StringComparison.Ordinal);
        }
        else if (left.Type == Runtime.ValueType.Bool && right.Type == Runtime.ValueType.Bool)
        {
            result = left.AsBool() == right.AsBool();
        }
        else
        {
            throw new RuntimeException("Unsupported types for equality comparison.");
        }

        _evaluationStack.Push(new Value(isEqual ? result : !result));
    }

    private void ExecuteComparison(Func<double, double, bool> operation)
    {
        Value right = _evaluationStack.Pop();
        Value left = _evaluationStack.Pop();
        _evaluationStack.Push(new Value(operation(left.AsNum(), right.AsNum())));
    }

    private Value ConvertToBool(Value value)
    {
        if (value.Type == Runtime.ValueType.Bool)
        {
            return value;
        }

        if (value.Type == Runtime.ValueType.Int)
        {
            return new Value(value.AsInt() != 0);
        }

        if (value.Type == Runtime.ValueType.Num)
        {
            return new Value(Math.Abs(value.AsNum()) > double.Epsilon);
        }

        if (value.Type == Runtime.ValueType.String)
        {
            return new Value(value.AsString().Length > 0);
        }

        throw new RuntimeException("Unsupported conversion to bool.");
    }

    private void CallUserFunction(string name)
    {
        if (!_functions.TryGetValue(name, out CompiledFunctionInfo? function))
        {
            throw new RuntimeException($"Unknown function '{name}'.");
        }

        List<Value> arguments = new(function.ParameterNames.Count);
        for (int i = 0; i < function.ParameterNames.Count; i++)
        {
            arguments.Add(_evaluationStack.Pop());
        }

        arguments.Reverse();

        Dictionary<string, VariableEntry> locals = new(StringComparer.Ordinal);
        for (int i = 0; i < function.ParameterNames.Count; i++)
        {
            locals[function.ParameterNames[i]] = new VariableEntry(
                arguments[i].Type,
                isConst: false,
                isInitialized: true,
                arguments[i]);
        }

        _callFrames.Push(new CallFrame(function, _instructionPointer, locals));
        _instructionPointer = function.EntryPoint;
    }

    private void ReturnFromFunction()
    {
        if (_callFrames.Count == 0)
        {
            throw new RuntimeException("Return outside of function frame.");
        }

        CallFrame frame = _callFrames.Pop();

        Value returnValue = Value.Void;
        if (!frame.Function.IsProcedure)
        {
            returnValue = _evaluationStack.Pop();
        }

        _instructionPointer = frame.ReturnAddress;

        if (!frame.Function.IsProcedure)
        {
            _evaluationStack.Push(returnValue);
        }
    }

    private void ExecuteMinOrMax(bool isMin)
    {
        int count = _evaluationStack.Pop().AsInt();

        List<Value> values = new(count);
        for (int i = 0; i < count; i++)
        {
            values.Add(_evaluationStack.Pop());
        }

        values.Reverse();

        if (values[0].Type == Runtime.ValueType.Int)
        {
            int result = values[0].AsInt();

            foreach (Value value in values.Skip(1))
            {
                result = isMin ? Math.Min(result, value.AsInt()) : Math.Max(result, value.AsInt());
            }

            _evaluationStack.Push(new Value(result));
            return;
        }

        double numResult = values[0].AsNum();

        foreach (Value value in values.Skip(1))
        {
            numResult = isMin ? Math.Min(numResult, value.AsNum()) : Math.Max(numResult, value.AsNum());
        }

        _evaluationStack.Push(new Value(numResult));
    }

    private sealed class CallFrame
    {
        public CallFrame(CompiledFunctionInfo function, int returnAddress, Dictionary<string, VariableEntry> variables)
        {
            Function = function;
            ReturnAddress = returnAddress;
            Variables = variables;
        }

        public CompiledFunctionInfo Function { get; }

        public int ReturnAddress { get; }

        public Dictionary<string, VariableEntry> Variables { get; }
    }
}