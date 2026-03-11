namespace Parser;

public class Row
{
	private readonly object[] _values;

	public Row(params object[] values)
	{
		_values = values;
	}

	public int ColumnCount => _values.Length;
	public object this[int index] => _values[index];
}