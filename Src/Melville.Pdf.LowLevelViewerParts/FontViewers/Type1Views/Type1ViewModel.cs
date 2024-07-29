using System.Drawing;
using Melville.Fonts.Type1TextParsers;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.Type1Views;

public partial class Type1ViewModel
{
    public Type1ViewModel(Type1GenericFont Font)
    {
        gf = new(() => new(Font, "Generic Font"));
        dict = new(() => new TextTreeViewModel(Font.Dictionary));
        GlyphView = new Type1CharstringViewModel(Font);
    }

    private readonly Lazy<GenericFontViewModel> gf;
    public GenericFontViewModel GenericFont => gf.Value;

    public Type1CharstringViewModel GlyphView { get; }

    private readonly Lazy<TextTreeViewModel> dict;
    public TextTreeViewModel Dictionary => dict.Value;
}