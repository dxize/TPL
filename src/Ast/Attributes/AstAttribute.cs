using System.Runtime.CompilerServices;

namespace Ast;

/// <summary>
/// Атрибут AST, устанавливаемый ровно один раз на этапе семантического анализа.
/// </summary>
public struct AstAttribute<T>
{
    private T _value;
    private bool _initialized;

    public T Get([CallerMemberName] string? memberName = null)
    {
        if (!_initialized)
        {
            throw new InvalidOperationException($"Attribute '{memberName}' of type {typeof(T)} is not set");
        }

        return _value;
    }

    public void Set(T value, [CallerMemberName] string? memberName = null)
    {
        if (_initialized)
        {
            throw new InvalidOperationException($"Attribute '{memberName}' of type {typeof(T)} already has a value");
        }

        _value = value;
        _initialized = true;
    }
}