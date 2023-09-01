using System;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Xunit;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.Values;

public class SharedContentDictionaryTest
{
    private readonly IPostscriptDictionary sut;

    public SharedContentDictionaryTest()
    {
        var dict = new PostscriptShortDictionary(3);
        dict.Put("A"u8, 1);
        dict.Put("B"u8, 2);
        dict.Put("C"u8, 3);

        sut = new SharedContentDictionary(dict);
    }

    [Fact]
    public void AccessDefault()
    {
        Assert.Equal(3, sut.Length);
        Assert.Equal(2, sut.GetAs<int>("B"u8));
    }
    [Fact]
    public void AccessOverridden()
    {
        sut.Put("B"u8, 4);
        Assert.Equal(4, sut.Length);
        Assert.Equal(4, sut.GetAs<int>("B"u8));
    }
    [Fact]
    public void CopyOverrides()
    {
        sut.CopyFrom(PostscriptValueFactory.CreateDictionary("B"u8, 4),
            PostscriptValueFactory.CreateNull());
        Assert.Equal(4, sut.GetAs<int>("B"u8));
    }

    [Fact]
    public void SimpleEmnumeration()
    {
        var cursor = sut.CreateForAllCursor();
        int pos = 0;
        var data = new PostscriptValue[2];
        Assert.True(cursor.TryGetItr(data.AsSpan(), ref pos));
        Assert.Equal("A", data[0].ToString());
        Assert.Equal("1", data[1].ToString());
        Assert.True(cursor.TryGetItr(data.AsSpan(), ref pos));
        Assert.Equal("B", data[0].ToString());
        Assert.Equal("2", data[1].ToString());
        Assert.True(cursor.TryGetItr(data.AsSpan(), ref pos));
        Assert.Equal("C", data[0].ToString());
        Assert.Equal("3", data[1].ToString());
        Assert.False(cursor.TryGetItr(data.AsSpan(), ref pos));
    }
    [Fact]
    public void OverriddenEmnumeration()
    {
        sut.Put("B"u8, 4);
        var cursor = sut.CreateForAllCursor();
        int pos = 0;
        var data = new PostscriptValue[2];
        Assert.True(cursor.TryGetItr(data.AsSpan(), ref pos));
        Assert.Equal("A", data[0].ToString());
        Assert.Equal("1", data[1].ToString());
        Assert.True(cursor.TryGetItr(data.AsSpan(), ref pos));
        Assert.Equal("C", data[0].ToString());
        Assert.Equal("3", data[1].ToString());
        Assert.True(cursor.TryGetItr(data.AsSpan(), ref pos));
        Assert.Equal("B", data[0].ToString());
        Assert.Equal("4", data[1].ToString());
        Assert.False(cursor.TryGetItr(data.AsSpan(), ref pos));
    }
}