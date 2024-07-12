using System.Drawing;
using System.Numerics;
using System.Windows;
using Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;
using Melville.Fonts.Type1TextParsers;
using Melville.INPC;
using Melville.MVVM.Wpf.ViewFrames;
using Melville.Pdf.LowLevelViewerParts.FontViewers.CFFGlyphViewers;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Composites;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.Type1Views;


public class Type1CharstringViewModel :CharStringViewModel, ICreateView
{
    private readonly Type1GenericFont font;

    public Type1CharstringViewModel(Type1GenericFont font)
    {
        this.font = font;
        PageSelector.MaxPage = font.GlyphCount-1;
        LoadNewGlyph();
    }

    protected override ValueTask RenderGlyphAsync(ICffGlyphTarget renderTemp)
    {
        if (font == null) return ValueTask.CompletedTask;
        return font.RenderToCffGlyphTarget(
            (uint)PageSelector.Page, renderTemp, Matrix3x2.Identity);
    }

    public UIElement View() => 
        new CffGlyphView() { DataContext = this };
}

public partial class Type1ViewModel
{
    public Type1ViewModel(Type1GenericFont Font)
    {
        gf = new(() => new(Font));
        dict = new(() => ExtractDictionary(Font.Dictionary).ToArray());
        GlyphView = new Type1CharstringViewModel(Font);
    }

    private readonly Lazy<GenericFontViewModel> gf;
    public GenericFontViewModel GenericFont => gf.Value;

    public Type1CharstringViewModel GlyphView { get; }

    private readonly Lazy<TextTree[]> dict;
    public TextTree[] Dictionary => dict.Value;

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
            return new TextTree(name, ExtractDictionary(dict).ToArray());
        if ((!(value.IsString || value.IsLiteralName || value.IsExecutedName)) 
            && value.TryGet(out IPostscriptArray? array))
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