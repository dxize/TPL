using Runtime;

using Semantics.Exceptions;

using VirtualMachine.Builtins;
using VirtualMachine.Instructions;

namespace VirtualMachine;

public sealed class DeaVM
{
    private readonly IReadOnlyList<Instruction> _instructions;
    private readonly BuiltinFunctions _builtins;
    private readonly IEnvironment _environment;
    private readonly Stack<Value> _evaluationStack;
    private readonly Dictionary<string, VariableEntry> _variables;
    private int _instructionPointer;

    public DeaVM(IEnvironment environment, IReadOnlyList<Instruction> instructions)
    {
        _environment = environment;
        _instructions = instructions;
        _builtins = new BuiltinFunctions(environment);
        _evaluationStack = [];
        _variables = new Dictionary<string, VariableEntry>(StringComparer.Ordinal);
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

                case InstructionCode.CallBuiltin:
                    ExecuteBuiltin(instruction.Operand);
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
            _ => throw new RuntimeException("Unsupported variable type tag."),
        };

        if (_variables.ContainsKey(name))
        {
            throw new RuntimeException($"Identifier '{name}' already declared.");
        }

        // Тип уже проверен на этапе семантики — используем значение как есть.
        _variables[name] = new VariableEntry(declaredType, isConstTag.AsInt() == 1, hasInitializer, hasInitializer ? value : Value.Void);
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
            _ when variable.Type == Runtime.ValueType.Int => int.TryParse(text, out int intValue)
                ? new Value(intValue)
                : throw new RuntimeException($"Input value '{text}' is not int."),
            _ when variable.Type == Runtime.ValueType.Num => double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double numValue)
                ? new Value(numValue)
                : throw new RuntimeException($"Input value '{text}' is not num."),
            _ when variable.Type == Runtime.ValueType.String => new Value(text),
            _ => throw new RuntimeException("Unsupported variable type for input."),
        };

        variable.SetValue(value);
    }

    private VariableEntry GetVariable(string name)
    {
        if (!_variables.TryGetValue(name, out VariableEntry? value))
        {
            throw new RuntimeException($"Identifier '{name}' is not declared.");
        }

        return value;
    }

    private void ExecuteAdd()
    {
        Value right = _evaluationStack.Pop();
        Value left = _evaluationStack.Pop();

        // Типы уже проверены на этапе семантики.
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
        // Типы уже проверены на этапе семантики.
        if (left.Type == Runtime.ValueType.Int && right.Type == Runtime.ValueType.Int)
        {
            _evaluationStack.Push(new Value(intOperation(left.AsInt(), right.AsInt())));
            return;
        }

        // Смешанные int/num или num/num — приводим к num.
        _evaluationStack.Push(new Value(numOperation(left.AsNum(), right.AsNum())));
    }

    private void ExecuteDivide()
    {
        Value right = _evaluationStack.Pop();
        Value left = _evaluationStack.Pop();

        // Типы уже проверены на этапе семантики.
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

        // Типы уже проверены на этапе семантики.
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

        // Типы уже проверены на этапе семантики.
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

        // Типы уже проверены на этапе семантики.
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

        // Тип уже проверен на этапе семантики.
        if (value.Type == Runtime.ValueType.Int)
        {
            _evaluationStack.Push(new Value(-value.AsInt()));
            return;
        }

        _evaluationStack.Push(new Value(-value.AsNum()));
    }

    private void ExecuteMinOrMax(bool isMin)
    {
        int count = _evaluationStack.Pop().AsInt();

        // Количество аргументов и типы уже проверены на этапе семантики.
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
}