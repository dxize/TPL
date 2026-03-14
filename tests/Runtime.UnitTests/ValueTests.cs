namespace Runtime.UnitTests;

public class ValueTests
{
    [Theory]
    [InlineData(42, "42")]
    [InlineData(0, "0")]
    public void Int_value_formats_for_output(int value, string expected)
    {
        Assert.Equal(expected, new Value(value).ToDisplayString());
    }

    [Fact]
    public void Num_value_can_be_read_as_num()
    {
        Value value = new(3.14);
        Assert.Equal(3.14, value.AsNum());
    }

    [Fact]
    public void String_value_can_be_read_as_string()
    {
        Value value = new("dea");
        Assert.Equal("dea", value.AsString());
    }

    [Fact]
    public void Void_value_is_marked_as_void()
    {
        Assert.True(Value.Void.IsVoid());
    }
}