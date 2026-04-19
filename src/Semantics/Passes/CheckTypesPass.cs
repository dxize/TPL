using Ast;
using Ast.Declarations;
using Ast.Expressions;

using Semantics.Exceptions;

namespace Semantics.Passes;

/// <summary>
/// Проход проверки типов: использует уже вычисленные ResultType
/// и проверяет совместимость типов в выражениях, вызовах и return.
/// </summary>
public sealed class CheckTypesPass : AbstractPass
{
    private DataType _currentReturnType = DataType.Void;

    /// <summary>
    /// Запоминает текущий тип возврата функции/процедуры.
    /// </summary>
    public override void Visit(FunctionDeclaration d)
    {
        DataType previousReturnType = _currentReturnType;
        _currentReturnType = d.ReturnType;
        base.Visit(d);
        _currentReturnType = previousReturnType;
    }

    /// <summary>
    /// Проверяет совместимость типа инициализатора с типом переменной.
    /// </summary>
    public override void Visit(VariableDeclarationExpression e)
    {
        base.Visit(e);

        if (e.Initializer is not null)
        {
            EnsureAssignable(e.Type, e.Initializer.ResultType, e.Name);
        }
    }

    /// <summary>
    /// Проверяет совместимость типа инициализатора с типом константы.
    /// </summary>
    public override void Visit(ConstantDeclarationExpression e)
    {
        base.Visit(e);
        EnsureAssignable(e.Type, e.Initializer.ResultType, e.Name);
    }

    /// <summary>
    /// Проверяет совместимость присваиваемого значения с типом переменной.
    /// </summary>
    public override void Visit(AssignmentExpression e)
    {
        base.Visit(e);
        EnsureAssignable(e.Symbol.Type, e.Value.ResultType, e.Name);
    }

    /// <summary>
    /// Проверяет builtin-вызовы и вызовы пользовательских функций.
    /// </summary>
    public override void Visit(CallExpression e)
    {
        base.Visit(e);

        if (e.IsBuiltin)
        {
            CheckBuiltinArgumentTypes(e);
            return;
        }

        if (e.Function!.IsProcedure)
        {
            throw new TypeErrorException($"Procedure '{e.Name}' cannot be used in expression context.");
        }

        CheckUserCallableArguments(e.Function, e.Arguments, e.Name);
    }

    /// <summary>
    /// Проверяет вызов процедуры как отдельной инструкции.
    /// </summary>
    public override void Visit(ProcedureCallStatement s)
    {
        base.Visit(s);

        if (!s.Procedure.IsProcedure)
        {
            throw new TypeErrorException($"Function '{s.Name}' cannot be used as a statement call.");
        }

        CheckUserCallableArguments(s.Procedure, s.Arguments, s.Name);
    }

    /// <summary>
    /// Проверяет корректность return относительно текущей функции/процедуры.
    /// </summary>
    public override void Visit(ReturnExpression e)
    {
        base.Visit(e);

        if (_currentReturnType == DataType.Void)
        {
            if (e.HasValue)
            {
                throw new TypeErrorException("Procedure cannot return a value.");
            }

            return;
        }

        if (!e.HasValue)
        {
            throw new TypeErrorException("Function must return a value.");
        }

        EnsureAssignable(_currentReturnType, e.Value!.ResultType, "return");
    }

    /// <summary>
    /// Проверяет, что условие if приводимо к bool.
    /// </summary>
    public override void Visit(IfStatement s)
    {
        base.Visit(s);
        EnsureBoolConvertible(s.Condition.ResultType, "if condition");
    }

    /// <summary>
    /// Проверяет количество и типы аргументов пользовательской функции/процедуры.
    /// </summary>
    private static void CheckUserCallableArguments(FunctionDeclaration function, List<Expression> arguments, string name)
    {
        if (function.Parameters.Count != arguments.Count)
        {
            throw new TypeErrorException($"'{name}' expects {function.Parameters.Count} argument(s), got {arguments.Count}.");
        }

        for (int i = 0; i < function.Parameters.Count; i++)
        {
            EnsureAssignable(function.Parameters[i].Type, arguments[i].ResultType, $"argument {i + 1} of '{name}'");
        }
    }

    /// <summary>
    /// Проверка количества и типов аргументов встроенных функций.
    /// </summary>
    private static void CheckBuiltinArgumentTypes(CallExpression call)
    {
        BuiltinInfo builtin = call.Builtin!;

        if (builtin.FixedArgCount is int expectedCount)
        {
            if (call.Arguments.Count != expectedCount)
            {
                throw new TypeErrorException($"'{builtin.Name}' expects {expectedCount} argument(s), got {call.Arguments.Count}.");
            }
        }
        else if (call.Arguments.Count < 2)
        {
            throw new TypeErrorException($"'{builtin.Name}' expects at least 2 arguments, got {call.Arguments.Count}.");
        }

        switch (builtin.Name)
        {
            case "len":
                CheckAreSameTypes("'len'", call.Arguments[0].ResultType, DataType.String);
                break;

            case "substr":
                CheckAreSameTypes("'substr' arg 1", call.Arguments[0].ResultType, DataType.String);
                CheckAreSameTypes("'substr' arg 2", call.Arguments[1].ResultType, DataType.Int);
                CheckAreSameTypes("'substr' arg 3", call.Arguments[2].ResultType, DataType.Int);
                break;

            case "abs":
                CheckIsNumeric("'abs'", call.Arguments[0].ResultType);
                break;

            case "min":
            case "max":
                CheckAllNumeric(builtin.Name, call.Arguments);
                CheckAllSameTypes(builtin.Name, call.Arguments);
                break;
        }
    }

    /// <summary>
    /// Проверяет, можно ли присвоить значение actual типу expected.
    /// </summary>
    private static void EnsureAssignable(DataType expected, DataType actual, string context)
    {
        if (expected != actual)
        {
            throw new TypeErrorException($"Cannot assign value of type {actual} to '{context}' of type {expected}.");
        }
    }

    /// <summary>
    /// Проверяет совпадение фактического и ожидаемого типа.
    /// </summary>
    private static void CheckAreSameTypes(string context, DataType actual, DataType expected)
    {
        if (actual != expected)
        {
            throw new TypeErrorException($"'{context}' expects {expected}, got {actual}.");
        }
    }

    /// <summary>
    /// Проверяет, что все аргументы имеют одинаковый тип.
    /// </summary>
    private static void CheckAllSameTypes(string functionName, List<Expression> arguments)
    {
        DataType firstType = arguments[0].ResultType;

        for (int i = 1; i < arguments.Count; i++)
        {
            if (arguments[i].ResultType != firstType)
            {
                throw new TypeErrorException(
                    $"'{functionName}' requires all arguments to be of the same type, expected {firstType}, got {arguments[i].ResultType} at argument {i + 1}.");
            }
        }
    }

    /// <summary>
    /// Проверяет, что тип является числовым.
    /// </summary>
    private static void CheckIsNumeric(string functionName, DataType type)
    {
        if (type is not (DataType.Int or DataType.Num))
        {
            throw new TypeErrorException($"'{functionName}' expects int/num, got {type}.");
        }
    }

    /// <summary>
    /// Проверяет, что все аргументы являются числовыми.
    /// </summary>
    private static void CheckAllNumeric(string functionName, List<Expression> arguments)
    {
        for (int i = 0; i < arguments.Count; i++)
        {
            if (arguments[i].ResultType is not (DataType.Int or DataType.Num))
            {
                throw new TypeErrorException($"'{functionName}' expects int/num arguments, got {arguments[i].ResultType}.");
            }
        }
    }

    /// <summary>
    /// Проверяет, что тип можно использовать как условие.
    /// </summary>
    private static void EnsureBoolConvertible(DataType type, string context)
    {
        if (type is not (DataType.Bool or DataType.Int or DataType.Num or DataType.String))
        {
            throw new TypeErrorException($"{context} expects bool-convertible value, got {type}.");
        }
    }
}