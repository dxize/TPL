namespace Parser;

public static class BuiltinFunctions
{
	private static readonly Dictionary<string, Func<List<decimal>, decimal>> Functions = new(StringComparer.OrdinalIgnoreCase)
	{
		{ "min", Min },
		{ "max", Max },
		{ "abs", Abs }
    };

	public static decimal Invoke(string name, List<decimal> arguments)
	{
		if (!Functions.TryGetValue(name, out Func<List<decimal>, decimal>? function))
		{
			throw new ArgumentException($"Unknown builtin function {name}");
		}

		return function(arguments);
	}

	private static decimal Min(List<decimal> arguments)
	{
		if (arguments.Count == 0)
			throw new ArgumentException("min requires at least one argument");
		return arguments.Min();
	}

	private static decimal Max(List<decimal> arguments)
	{
		if (arguments.Count == 0)
			throw new ArgumentException("max requires at least one argument");
		return arguments.Max();
	}

	private static decimal Abs(List<decimal> arguments)
	{
		if (arguments.Count != 1)
			throw new ArgumentException("abs requires exactly one argument");
		return Math.Abs(arguments[0]);
	}
}