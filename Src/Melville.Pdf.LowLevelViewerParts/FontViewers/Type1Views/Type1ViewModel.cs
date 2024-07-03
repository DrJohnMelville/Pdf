using Melville.Fonts.Type1TextParsers;
using Melville.INPC;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.Type1Views;

public partial class Type1ViewModel
{
    public Type1ViewModel(Type1GenericFont Font)
    {
        gf = new(() => new(Font));
        dict = new(() => ExtractDictionary(Font.Dictionary).ToArray());
    }

    private readonly Lazy<GenericFontViewModel> gf;
    public GenericFontViewModel GenericFont => gf.Value;

    private readonly Lazy<TextTree[]> dict;
    public TextTree[] Dictionary => dict.Value;

    private static IEnumerable<TextTree> ExtractDictionary(
        IPostscriptDictionary? fontDictionary)
    {
        var iter = fontDictionary.CreateForAllCursor();
        var values  = new PostscriptValue[2];
        var pos = 0;
        while (iter.TryGetItr(values, ref pos))
        {
            yield return CreateTextTree(values[0].ToString(), values[1]);
        }
    }

    private static TextTree CreateTextTree(string name, PostscriptValue value)
    {
        if (value.TryGet(out IPostscriptDictionary dict))
            return new TextTree(name, ExtractDictionary(dict).ToArray());
        if ((!(value.IsString || value.IsLiteralName || value.IsExecutedName)) 
            && value.TryGet(out IPostscriptArray array))
            return new TextTree(name, array
                .Select((item, i) => CreateTextTree(i.ToString(), item)).ToArray());
        return new TextTree($"{name}: {value.ToString()}", []);
    }
}

public partial class TextTree
{
    [FromConstructor] public string Text { get; }
    [FromConstructor] public TextTree[] Children { get; }
}