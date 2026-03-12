using Ast;

namespace Execution;

public class Scope
{
    private readonly Dictionary<string, (object value, DataType type)> _variables = [];

    public bool TryGetVariable(string name, out object value)
    {
        if (_variables.TryGetValue(name, out var entry))
        {
            value = entry.value;
            return true;
        }

        value = null!;
        return false;
    }

    public bool TryGetVariableType(string name, out DataType type)
    {
        if (_variables.TryGetValue(name, out var entry))
        {
            type = entry.type;
            return true;
        }

        type = default;
        return false;
    }

    public bool TryAssignVariable(string name, object value)
    {
        if (_variables.TryGetValue(name, out var entry))
        {
            _variables[name] = (value, entry.type);
            return true;
        }

        return false;
    }

    public bool TryDefineVariable(string name, object value, DataType type)
    {
        return _variables.TryAdd(name, (value, type));
    }
}