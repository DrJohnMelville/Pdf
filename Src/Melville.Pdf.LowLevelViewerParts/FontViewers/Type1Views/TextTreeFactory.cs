using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.Type1Views;

internal class TextTreeFactory
{
    public IEnumerable<TextTree> ExtractDictionary(
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

    private int depth = 0;
    public TextTree CreateTextTree(string name, PostscriptValue value)
    {
        if (depth > 20) 
            return new TextTree("Too Deep", Array.Empty<TextTree>());

        try
        {
            depth++;
            if (value.TryGet(out IPostscriptDictionary? dict))
                return ReadNamedDictionary(name, dict);
            if ((!(value.IsString || value.IsLiteralName || value.IsExecutedName)) 
                && value.TryGet(out IPostscriptArray? array))
                return new TextTree($"{name} Array [{array.Length}]", array
                    .Select((item, i) => CreateTextTree(i.ToString(), item)).ToArray());
            return new TextTree($"{name}: {value.ToString()}", []);
        }
        finally
        {
            depth--;
        }
    }

    public TextTree ReadNamedDictionary(string name, IPostscriptDictionary dict)
    {
        return new TextTree($"{name} Dictionary [{dict.Length}]", ExtractDictionary(dict).ToArray());
    }

}