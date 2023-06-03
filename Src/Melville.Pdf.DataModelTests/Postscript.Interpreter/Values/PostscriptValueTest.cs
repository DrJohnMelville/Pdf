using System;
using System.Runtime.InteropServices;
using Melville.Pdf.DataModelTests.Writer.Lzw;
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
    public void StringTest(string stringValue) => DoStringTest(stringValue, StringKind.String,
        s => $"({s})");

    [Theory]
    [InlineData("123456789012345678")]
    [InlineData("1234567890123456789")]
    public void NameTest(string stringValue) => DoStringTest(stringValue, StringKind.Name, s=>s);
    [Theory]
    [InlineData("123456789012345678")]
    [InlineData("1234567890123456789")]
    public void LiteralNameTest(string stringValue) => 
        DoStringTest(stringValue, StringKind.LiteralName, s=>"/"+s);

    private static void DoStringTest(string stringValue, StringKind kind, 
        Func<string, string>  stringType)
    {
        var value = PostscriptValueFactory.CreateString(stringValue, kind);
        Assert.Equal(stringType(stringValue), value.Get<string>());
        Assert.Equal(kind, value.Get<StringKind>());
    }

    [Fact]
    public void ArrayTest()
    {
        var value = PostscriptValueFactory.CreateArray(
            PostscriptValueFactory.Create(true),
            PostscriptValueFactory.Create(false),
            PostscriptValueFactory.Create(10)
        );
        Assert.Equal("[true, false, 10]", value.Get<string>());

        Assert.Equal(10, value.Get<IPostscriptComposite>().Get(
            PostscriptValueFactory.Create(2)).Get<long>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DictionaryToStringTest(bool longDict)
    {
        var value = CreateDictionary(longDict);
        Assert.Equal("""
            <<
                /A: 1
                /B: 2
                /Charlie: 3
            >>
            """, value.ToString());
    }
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DictionaryAccessTest(bool longDict)
    {
        var value = CreateDictionary(longDict);
        Assert.Equal(1, value.Get<IPostscriptComposite>()
            .Get(PostscriptValueFactory.CreateString("A", StringKind.LiteralName))
            .Get<int>());
        Assert.Equal(2, value.Get<IPostscriptComposite>()
            .Get(PostscriptValueFactory.CreateString("B", StringKind.LiteralName))
            .Get<int>());
        Assert.Equal(3, value.Get<IPostscriptComposite>()
            .Get(PostscriptValueFactory.CreateString("Charlie", StringKind.LiteralName))
            .Get<int>());
    }

    private static PostscriptValue CreateDictionary(bool longDict)
    {
        var parameters = new[]
        {
            PostscriptValueFactory.CreateString("A", StringKind.LiteralName),
            PostscriptValueFactory.Create(1),
            PostscriptValueFactory.CreateString("B", StringKind.LiteralName),
            PostscriptValueFactory.Create(2),
            PostscriptValueFactory.CreateString("Charlie", StringKind.LiteralName),
            PostscriptValueFactory.Create(3)
        };

        return PostscriptValueFactory.CreateDictionary(parameters);
    }
}