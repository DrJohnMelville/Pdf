using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using System.Collections;
using System.Collections.Generic;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.Values;

public class CompositesTest
{
  public IEnumerable<(PostscriptValue, PostscriptValue, PostscriptValue, IPostscriptComposite)> Tests() => 
        [
            (PostscriptValueFactory.Create(0),PostscriptValueFactory.Create(1),PostscriptValueFactory.Create(2), new PostscriptArray(3)),
            (Str("Cab"),Str("Apple"),Str("Bongo"), new PostscriptShortDictionary(5)),
            (Str("Cab"),Str("Apple"),Str("Bongo"), new PostscriptLongDictionary())
        ];

    static PostscriptValue Str(string str) => PostscriptValueFactory.CreateString(str, StringKind.LiteralName);

    [Test]
    [MethodDataSource(nameof(Tests))]
    public void AddAndRemove1(PostscriptValue Name1, PostscriptValue Name2, PostscriptValue Name3, IPostscriptComposite sut)
    {
        sut.Put(Name1, PostscriptValueFactory.CreateString("Hello", StringKind.String));
        Assert.True(sut.TryGet(Name1, out var result));
        Assert.Equal("(Hello)", result.ToString());
    }

    [Test]
    [MethodDataSource(nameof(Tests))]
    public void AddAndRemove2(PostscriptValue Name1, PostscriptValue Name2, PostscriptValue Name3, IPostscriptComposite sut)
    {
        sut.Put(Name1, PostscriptValueFactory.CreateString("Hello", StringKind.String));
        sut.Put(Name2, PostscriptValueFactory.CreateString("World", StringKind.String));
        Assert.True(sut.TryGet(Name1, out var result));
        Assert.Equal("(Hello)", result.ToString());
        Assert.Equal("(World)", sut.Get(Name2).ToString());
    }
}