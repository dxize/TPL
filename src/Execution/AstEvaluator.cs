using Ast;
using Ast.Declarations;
using Ast.Expressions;
using Execution.Exceptions;
using Lexer;

namespace Execution;

public class AstEvaluator(Context context, IEnvironment environment) : IAstVisitor
{
    private readonly Stack<object> _values = [];

    public object Evaluate(AstNode node)
    {
        if (_values.Count > 0)
        {
            throw new InvalidOperationException(
                $"Evaluation stack must be empty, but contains {_values.Count} values: {string.Join(", ", _values)}"
            );
        }

        node.Accept(this);

        return _values.Count switch
        {
            0 => throw new InvalidOperationException(
                "Evaluator logical error: the stack has no evaluation result"
            ),
            > 1 => throw new InvalidOperationException(
                $"Evaluator logical error: expected 1 value, got {_values.Count} values: {string.Join(", ", _values)}"
            ),
            _ => _values.Pop(),
        };
    }

    public void Execute(AstNode node)
    {
        node.Accept(this);
        if (_values.Count > 0)
        {
            _values.Pop();
        }
    }

    public void Visit(BinaryOperationExpression e)
    {
        if (e.Operation == BinaryOperation.LogicalAnd)
        {
            e.Left.Accept(this);
            if (!ToBool(_values.Pop()))
            {
                _values.Push(false);
                return;
            }
            e.Right.Accept(this);
            _values.Push(ToBool(_values.Pop()));
            return;
        }

        if (e.Operation == BinaryOperation.LogicalOr)
        {
            e.Left.Accept(this);
            if (ToBool(_values.Pop()))
            {
                _values.Push(true);
                return;
            }
            e.Right.Accept(this);
            _values.Push(ToBool(_values.Pop()));
            return;
        }

        e.Left.Accept(this);
        e.Right.Accept(this);
        object right = _values.Pop();
        object left = _values.Pop();

        switch (e.Operation)
        {
            case BinaryOperation.Plus:
                if (left is string || right is string)
                    _values.Push(left.ToString() + right.ToString());
                else if (left is double || right is double)
                    _values.Push(Convert.ToDouble(left) + Convert.ToDouble(right));
                else
                    _values.Push(Convert.ToInt32(left) + Convert.ToInt32(right));
                break;
            case BinaryOperation.Minus:
                if (left is double || right is double)
                    _values.Push(Convert.ToDouble(left) - Convert.ToDouble(right));
                else
                    _values.Push(Convert.ToInt32(left) - Convert.ToInt32(right));
                break;
            case BinaryOperation.Multiply:
                if (left is double || right is double)
                    _values.Push(Convert.ToDouble(left) * Convert.ToDouble(right));
                else
                    _values.Push(Convert.ToInt32(left) * Convert.ToInt32(right));
                break;
            case BinaryOperation.Divide:
                double dRight = Convert.ToDouble(right);
                if (dRight == 0.0) throw new DivideByZeroException();
                _values.Push(Convert.ToDouble(left) / dRight);
                break;
            case BinaryOperation.IntegerDivide:
                int iRight = Convert.ToInt32(right);
                if (iRight == 0) throw new DivideByZeroException();
                _values.Push(Convert.ToInt32(left) / iRight);
                break;
            case BinaryOperation.Modulo:
                int modRight = Convert.ToInt32(right);
                if (modRight == 0) throw new DivideByZeroException();
                _values.Push(Convert.ToInt32(left) % modRight);
                break;
            case BinaryOperation.Power:
                _values.Push(Math.Pow(Convert.ToDouble(left), Convert.ToDouble(right)));
                break;
            case BinaryOperation.LessThan:
                _values.Push(Compare(left, right) < 0);
                break;
            case BinaryOperation.GreaterThan:
                _values.Push(Compare(left, right) > 0);
                break;
            case BinaryOperation.LessThanOrEqual:
                _values.Push(Compare(left, right) <= 0);
                break;
            case BinaryOperation.GreaterThanOrEqual:
                _values.Push(Compare(left, right) >= 0);
                break;
            case BinaryOperation.Equal:
                _values.Push(Compare(left, right) == 0);
                break;
            case BinaryOperation.NotEqual:
                _values.Push(Compare(left, right) != 0);
                break;
            default:
                throw new NotImplementedException($"Unknown binary operation {e.Operation}");
        }
    }

    private int Compare(object left, object right)
    {
        if (left is string lStr && right is string rStr) return string.CompareOrdinal(lStr, rStr);
        if (left is bool lBool && right is bool rBool) return lBool.CompareTo(rBool);
        if (left is double || right is double) return Convert.ToDouble(left).CompareTo(Convert.ToDouble(right));
        return Convert.ToInt32(left).CompareTo(Convert.ToInt32(right));
    }

    public void Visit(UnaryOperationExpression e)
    {
        e.Operand.Accept(this);
        object value = _values.Pop();

        switch (e.Operation)
        {
            case UnaryOperation.Plus:
                _values.Push(value);
                break;
            case UnaryOperation.Minus:
                if (value is double d) _values.Push(-d);
                else _values.Push(-Convert.ToInt32(value));
                break;
            case UnaryOperation.LogicalNot:
                _values.Push(!ToBool(value));
                break;
            default:
                throw new NotImplementedException($"Unknown unary operation {e.Operation}");
        }
    }

    private bool ToBool(object value)
    {
        if (value is bool b) return b;
        if (value is int i) return i != 0;
        if (value is double d) return d != 0.0;
        if (value is string s) return !string.IsNullOrEmpty(s);
        return false;
    }

    public void Visit(LiteralExpression e)
    {
        _values.Push(e.Value);
    }

    public void Visit(VariableExpression e)
    {
        _values.Push(context.GetValue(e.Name));
    }

    public void Visit(FunctionCall call)
    {
        List<object> argValues = new();
        foreach (Expression arg in call.Arguments)
        {
            arg.Accept(this);
            argValues.Add(_values.Pop());
        }

        string lowerName = call.FunctionName.ToLower();
        if (IsBuiltInFunction(lowerName))
        {
            object result = lowerName switch
            {
                "abs" => valAbs(argValues[0]),
                "max" => valMax(argValues),
                "min" => valMin(argValues),
                "len" => (int)argValues[0].ToString()!.Length,
                "substr" => valSubstr(argValues),
                _ => throw new Exception($"Unknown built-in function: {call.FunctionName}"),
            };
            _values.Push(result);
        }
        else
        {
            FunctionDeclaration function = context.GetFunction(call.FunctionName);

            context.PushScope(new Scope());
            try
            {
                for (int i = 0; i < function.Parameters.Count; i++)
                {
                    context.DefineVariable(function.Parameters[i].Name, argValues[i], function.Parameters[i].Type);
                }

                object returnValue = 0;

                try
                {
                    foreach (AstNode statement in function.Body)
                    {
                        statement.Accept(this);
                        if (_values.Count > 0 && !(statement is ReturnExpression)) _values.Pop();
                    }
                }
                catch (ReturnException ret)
                {
                    returnValue = ret.ReturnValue;
                }

                while (_values.Count > 0) _values.Pop();
                _values.Push(returnValue);
            }
            finally
            {
                context.PopScope();
            }
        }
    }

    private object valAbs(object val) 
    {
        if (val is double d) return Math.Abs(d);
        return Math.Abs(Convert.ToInt32(val));
    }
    
    private object valMax(List<object> vals) 
    {
        if (vals.Any(v => v is double)) return vals.Max(v => Convert.ToDouble(v));
        return vals.Max(v => Convert.ToInt32(v));
    }
    
    private object valMin(List<object> vals) 
    {
        if (vals.Any(v => v is double)) return vals.Min(v => Convert.ToDouble(v));
        return vals.Min(v => Convert.ToInt32(v));
    }


    private object valSubstr(List<object> args)
    {
        string s = args[0].ToString()!;
        int start = Convert.ToInt32(args[1]);
        int length = Convert.ToInt32(args[2]);
        if (start < 0 || length < 0 || start > s.Length || start + length > s.Length) throw new ArgumentOutOfRangeException("substr indices out of range");
        return s.Substring(start, length);
    }

    public void Visit(VariableScopeExpression e)
    {
        context.PushScope(new Scope());
        try
        {
            foreach (VariableDeclaration variable in e.Variables)
            {
                variable.Accept(this);
                _values.Pop();
            }
            e.Expression.Accept(this);
        }
        finally
        {
            context.PopScope();
        }
    }

    public void Visit(AssignmentExpression e)
    {
        e.Value.Accept(this);
        object value = _values.Pop();
        context.AssignVariable(e.Name, value);
        _values.Push(value);
    }

    public void Visit(PrintExpression s)
    {
        foreach (Expression arg in s.Arguments)
        {
            arg.Accept(this);
            environment.AddResult(_values.Pop());
        }
        _values.Push(0);
    }

    public void Visit(InputExpression s)
    {
        DataType type = context.GetVariableType(s.VariableName);
        object value = environment.ReadInput(type);
        context.AssignVariable(s.VariableName, value);
        _values.Push(0);
    }

    public void Visit(IfExpression s)
    {
        s.Condition.Accept(this);
        if (ToBool(_values.Pop()))
        {
            foreach (AstNode statement in s.ThenBranch)
            {
                statement.Accept(this);
                if (_values.Count > 0 && !(statement is ReturnExpression)) _values.Pop();
            }
        }
        _values.Push(0);
    }

    public void Visit(IfElseExpression s)
    {
        s.Condition.Accept(this);
        if (ToBool(_values.Pop()))
        {
            foreach (AstNode statement in s.ThenBranch)
            {
                statement.Accept(this);
                if (_values.Count > 0 && !(statement is ReturnExpression)) _values.Pop();
            }
        }
        else
        {
            foreach (AstNode statement in s.ElseBranch)
            {
                statement.Accept(this);
                if (_values.Count > 0 && !(statement is ReturnExpression)) _values.Pop();
            }
        }
        _values.Push(0);
    }

    public void Visit(WhileExpression s)
    {
        bool isNotBreaked = true;
        while (isNotBreaked)
        {
            s.Condition.Accept(this);
            if (!ToBool(_values.Pop())) break;

            foreach (AstNode statement in s.Body)
            {
                try
                {
                    statement.Accept(this);
                    if (_values.Count > 0 && !(statement is ReturnExpression)) _values.Pop();
                }
                catch (ContinueException) { goto next_iteration; }
                catch (BreakException) { isNotBreaked = false; break; }
            }
            next_iteration:;
        }
        _values.Push(0);
    }

    public void Visit(ForLoopExpression e)
    {
        context.PushScope(new Scope());
        try
        {
            e.StartValue.Accept(this);
            context.DefineVariable(e.IteratorName, _values.Pop(), DataType.Int);

            bool isNotBreaked = true;
            while (isNotBreaked)
            {
                e.EndCondition.Accept(this);
                if (!ToBool(_values.Pop())) break;

                foreach (AstNode statement in e.Body)
                {
                    try
                    {
                        statement.Accept(this);
                        if (_values.Count > 0 && !(statement is ReturnExpression)) _values.Pop();
                    }
                    catch (ContinueException) { goto next_for; }
                    catch (BreakException) { isNotBreaked = false; break; }
                }
                next_for:
                if (e.StepValue != null)
                {
                    e.StepValue.Accept(this);
                    if (_values.Count > 0) _values.Pop();
                }
            }
            _values.Push(0);
        }
        finally
        {
            context.PopScope();
        }
    }

    public void Visit(BreakExpression s) => throw new BreakException();
    public void Visit(ContinueExpression s) => throw new ContinueException();

    public void Visit(ReturnExpression s)
    {
        object returnValue = 0;
        if (s.Value != null)
        {
            s.Value.Accept(this);
            returnValue = _values.Pop();
        }
        throw new ReturnException(returnValue);
    }

    public void Visit(VariableDeclaration d)
    {
        object value = d.Type switch {
            DataType.Int => 0,
            DataType.Num => 0.0,
            DataType.String => "",
            DataType.Bool => false,
            _ => 0
        };

        if (d.Value != null)
        {
            d.Value.Accept(this);
            value = _values.Pop();
        }

        context.DefineVariable(d.Name, value, d.Type);
        _values.Push(0); // Declarations return 0 or similar
    }

    public void Visit(ConstantDeclaration d)
    {
        d.Value.Accept(this);
        object value = _values.Pop();
        context.DefineConstant(d.Name, value, d.Type);
        _values.Push(0);
    }

    public void Visit(FunctionDeclaration d)
    {
        context.DefineFunction(d);
        _values.Push(0);
    }

    private bool IsBuiltInFunction(string name) => name == "abs" || name == "max" || name == "min" || name == "len" || name == "substr";
}
