using Ast;
using Ast.Declarations;

namespace Execution;

/// <summary>
/// Контекст выполнения программы (все переменные, константы и другие символы).
/// </summary>
public class Context
{
    private readonly Stack<Scope> _scopes = [];
    private readonly Dictionary<string, (object value, DataType type)> _constants = [];
    private readonly Dictionary<string, FunctionDeclaration> _functions = [];

    public Context()
    {
        PushScope(new Scope());
    }

    public void PushScope(Scope scope)
    {
        _scopes.Push(scope);
    }

    public void PopScope()
    {
        if (_scopes.Count <= 1)
        {
            throw new InvalidOperationException("Cannot pop the global scope");
        }

        _scopes.Pop();
    }

    /// <summary>
    /// Возвращает значение переменной или константы.
    /// </summary>
    public object GetValue(string name)
    {
        foreach (Scope s in _scopes)
        {
            if (s.TryGetVariable(name, out object variable))
            {
                return variable;
            }
        }

        if (_constants.TryGetValue(name, out var entry))
        {
            return entry.value;
        }

        throw new ArgumentException($"Variable '{name}' is not defined");
    }

    public DataType GetVariableType(string name)
    {
        foreach (Scope s in _scopes)
        {
            if (s.TryGetVariableType(name, out DataType type))
            {
                return type;
            }
        }

        if (_constants.TryGetValue(name, out var entry))
        {
            return entry.type;
        }

        throw new ArgumentException($"Variable '{name}' is not defined");
    }
    /// <summary>
    /// Присваивает (изменяет) значение переменной.
    /// </summary>
    public void AssignVariable(string name, object value)
    {
        foreach (Scope s in _scopes)
        {
            if (s.TryAssignVariable(name, value))
            {
                return;
            }
        }

        throw new ArgumentException($"Variable '{name}' is not defined");
    }

    public void DefineVariable(string name, object value, DataType type)
    {
        if (!_scopes.Peek().TryDefineVariable(name, value, type))
        {
            throw new ArgumentException($"Variable '{name}' is already defined in this scope");
        }
    }

    public void DefineConstant(string name, object value, DataType type)
    {
        if (!_constants.TryAdd(name, (value, type)))
        {
            throw new ArgumentException($"Constant '{name}' is already defined");
        }
    }

    public FunctionDeclaration GetFunction(string name)
    {
        if (_functions.TryGetValue(name, out FunctionDeclaration? function))
        {
            return function;
        }

        throw new ArgumentException($"Function '{name}' is not defined");
    }

    public void DefineFunction(FunctionDeclaration function)
    {
        if (!_functions.TryAdd(function.Name, function))
        {
            throw new ArgumentException($"Function '{function.Name}' is already defined");
        }
    }
}