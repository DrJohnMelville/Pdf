using Melville.CSJ2K.j2k.wavelet.analysis;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;
using Xunit;

namespace Melville.Pdf.DataModelTests.Postscript.Interpreter.Values;

public abstract class CompositesTest
{
    protected abstract PostscriptValue Name1 {get;}
    protected abstract PostscriptValue Name2 {get;}
    protected abstract PostscriptValue Name3 {get;}

    protected abstract IPostscriptComposite CreateEmpty();

    [Fact]
    public void AddAndRemove1()
    {
        var sut = CreateEmpty();
        sut.Add(Name1, PostscriptValueFactory.CreateString("Hello", StringKind.String));
        Assert.True(sut.TryGet(Name1, out var result));
        Assert.Equal("(Hello)", result.Get<string>());
    }

    [Fact]
    public void AddAndRemove2()
    {
        var sut = CreateEmpty();
        sut.Add(Name1, PostscriptValueFactory.CreateString("Hello", StringKind.String));
        sut.Add(Name2, PostscriptValueFactory.CreateString("World", StringKind.String));
        Assert.True(sut.TryGet(Name1, out var result));
        Assert.Equal("(Hello)", result.Get<string>());
        Assert.Equal("(World)", sut.Get(Name2).Get<string>());
    }
}

public class ArrayTest : CompositesTest
{
    protected override PostscriptValue Name1 => PostscriptValueFactory.Create(0);
    protected override PostscriptValue Name2 => PostscriptValueFactory.Create(1);
    protected override PostscriptValue Name3 => PostscriptValueFactory.Create(2);
    protected override IPostscriptComposite CreateEmpty() => new PostscriptArray(3);
}

public abstract class DictionaryTest: CompositesTest
{
    protected override PostscriptValue Name1 => 
        PostscriptValueFactory.CreateString("Cab", StringKind.LiteralName);
    protected override PostscriptValue Name2 => 
        PostscriptValueFactory.CreateString("Apple", StringKind.LiteralName);
    protected override PostscriptValue Name3 => 
        PostscriptValueFactory.CreateString("Bongo", StringKind.LiteralName);
}

public class ShortDictionaryTest : DictionaryTest
{
    protected override IPostscriptComposite CreateEmpty() => new PostscriptShortDictionary();
}

public class LongDictionaryTest : DictionaryTest
{
    protected override IPostscriptComposite CreateEmpty() => new PostscriptLongDictionary();
}