using Melville.Postscript.Interpreter.Values;
using Xunit;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.Values;

public class PostscriptValueTest
{
    [Fact]
    public void VoidValueIsVoid()
    {
        var value = PostscriptValueFactory.CreateNull();
        Assert.True(value.IsNull);
        Assert.Equal("<Null>", value.ToString());
        Assert.False(value.TryGet<long>(out var _));
        Assert.Throws<InvalidPostscriptTypeException>(()=>value.Get<long>());
    }

    [Fact]
    public void MarkObject()
    {
        var value = PostscriptValueFactory.CreateMark();
        Assert.True(value.IsMark);
        Assert.Equal("<Mark Object>", value.ToString());
    }

    [Theory]
    [InlineData(long.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(long.MaxValue)]
    public void TestIntegerValue(long number)
    {
        var value = PostscriptValueFactory.Create(number);
        Assert.False(value.IsNull);
        Assert.Equal(number, value.Get<long>());
        Assert.True(value.TryGet<long>(out var result));
        Assert.Equal(number, result);
        Assert.Equal(number.ToString(), value.ToString());
        Assert.Equal((double)number, value.Get<double>());
    }

    [Theory]
    [InlineData(0,0)]
    [InlineData(1,1)]
    [InlineData(1.49,1)]
    [InlineData(1.5,2)]
    [InlineData(-1.49,-1)]
    [InlineData(-1.5,-2)]
    public void TestDoubleValue(double number, long rounded)
    {
        var value = PostscriptValueFactory.Create(number);
        Assert.Equal(rounded, value.Get<long>());
        Assert.True(value.TryGet<double>(out var result));
        Assert.Equal(number, result);
        Assert.Equal(number.ToString(), value.ToString());
        Assert.Equal(number, value.Get<double>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void TestBooleanValue(bool booleanValue)
    {
        var value = PostscriptValueFactory.Create(booleanValue);
        Assert.Equal(booleanValue, value.Get<bool>());
        Assert.Equal(booleanValue.ToString().ToLower(), value.Get<string>());
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("")]
    [InlineData("123456789012345678")]
    [InlineData("1234567890123456789")]
    public void StringTest(string stringValue) => DoStringTest(stringValue, StringKind.String);

    [Theory]
    [InlineData("123456789012345678")]
    [InlineData("1234567890123456789")]
    public void NameTest(string stringValue) => DoStringTest(stringValue, StringKind.Name);
    [Theory]
    [InlineData("123456789012345678")]
    [InlineData("1234567890123456789")]
    public void LiteralNameTest(string stringValue) => 
        DoStringTest(stringValue, StringKind.LiteralName);

    private static void DoStringTest(string stringValue, StringKind kind)
    {
        var value = PostscriptValueFactory.CreateString(stringValue, kind);
        Assert.Equal(stringValue, value.Get<string>());
        Assert.Equal(kind, value.Get<StringKind>());
    }
}