using Melville.INPC;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.Type1Views;

public partial class TextTree
{
    [FromConstructor] public string Text { get; }
    [FromConstructor] public TextTree[] Children { get; }
}
public partial class TextTreeViewModel
{
    public TextTreeViewModel(IPostscriptDictionary? fontDictionary):
        this(ExtractDictionary(fontDictionary).ToArray())
    {
    }

    [FromConstructor] public TextTree[] Tree { get; }

    public static TextTreeViewModel ReadOperandStack(OperandStack stack)
    {
        var span = stack.CollectionAsSpan();
        var ret = new TextTree[span.Length];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = CreateTextTree($"{i}: " , span[^(1+i)]);
        }

        return new TextTreeViewModel(ret);
    }
    public static TextTreeViewModel ReadDictionaryStack(DictionaryStack stack)
    {
        var span = stack.CollectionAsSpan();
        var ret = new TextTree[span.Length];
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = ReadNamedDictionary($"{i}: " , span[^(1+i)]);
        }

        return new TextTreeViewModel(ret);
    }

    private static IEnumerable<TextTree> ExtractDictionary(
        IPostscriptDictionary? fontDictionary)
    {
        if (fontDictionary == null) yield break;
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
        if (value.TryGet(out IPostscriptDictionary? dict))
            return ReadNamedDictionary(name, dict);
        if ((!(value.IsString || value.IsLiteralName || value.IsExecutedName)) 
            && value.TryGet(out IPostscriptArray? array))
            return new TextTree($"{name} Array [{array.Length}]", array
                .Select((item, i) => CreateTextTree(i.ToString(), item)).ToArray());
        return new TextTree($"{name}: {value.ToString()}", []);
    }

    private static TextTree ReadNamedDictionary(string name, IPostscriptDictionary dict)
    {
        return new TextTree($"{name} Dictionary [{dict.Length}]", ExtractDictionary(dict).ToArray());
    }
}