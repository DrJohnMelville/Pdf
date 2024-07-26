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
    public static readonly TextTreeViewModel Empty = new TextTreeViewModel([]);
    public TextTreeViewModel(IPostscriptDictionary? fontDictionary):
        this(new TextTreeFactory().ExtractDictionary(fontDictionary).ToArray())
    {
    }

    [FromConstructor] public TextTree[] Tree { get; }

    public static TextTreeViewModel ReadOperandStack(OperandStack stack)
    {
        var span = stack.CollectionAsSpan();
        var ret = new TextTree[span.Length];
        var factory = new TextTreeFactory();
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = factory.CreateTextTree($"{i}: " , span[^(1+i)]);
        }

        return new TextTreeViewModel(ret);
    }
    public static TextTreeViewModel ReadDictionaryStack(DictionaryStack stack)
    {
        var span = stack.CollectionAsSpan();
        var ret = new TextTree[span.Length];
        var factory = new TextTreeFactory();
        for (int i = 0; i < ret.Length; i++)
        {
            ret[i] = factory.ReadNamedDictionary($"{i}: " , span[^(1+i)]);
        }

        return new TextTreeViewModel(ret);
    }
}