using Runtime;

namespace VirtualMachine.Builtins;

public sealed class BuiltinFunctions
{
    private readonly IEnvironment _environment;

    public BuiltinFunctions(IEnvironment environment)
    {
        _environment = environment;
    }

    public void Print(Value value)
    {
        _environment.Print(value.ToDisplayString());
    }
}