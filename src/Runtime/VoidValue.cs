namespace Runtime;

public readonly record struct VoidValue
{
    public static readonly VoidValue Value = default;

    public override string ToString() => "<void>";
}
