using System.Collections.Generic;
using System.Reflection;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Xunit;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.InterpreterState;

public class DictionaryStackTest
{
    private readonly DictionaryStack item = new();

    [Fact]
    public void OneLevelLookup()
    {
        item.Push(PostscriptValueFactory.CreateLongDictionary(
            "/Key1", 1,
            "/Key2", 2));

        Assert.True(item.TryGet("/Key2", out var result));
        Assert.Equal(2, result.Get<int>());
        item.TryGet("/Key3", out var _);
    }
    [Fact]
    public void TwoLevelLookup()
    {
        item.Push(PostscriptValueFactory.CreateLongDictionary(
            "/Key1", 1,
            "/Key2", 2));
        item.Push(PostscriptValueFactory.CreateLongDictionary(
            "/Key1", 3,
            "/Key3", 4));


        Assert.Equal(3, item.Get("/Key1").Get<int>());
        Assert.Equal(4, item.Get("/Key3").Get<int>());
        Assert.Equal(2, item.Get("/Key2").Get<int>());
        item.TryGet("/Key10", out var _);
    }
}