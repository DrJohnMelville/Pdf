using Melville.Fonts;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class GenericFontViewModel
{
    [FromConstructor] public IGenericFont Font { get; }
    public string Title => "Generic Font View";
    public string? ToolTip => null;
    [AutoNotify] private object? glyphViewModel = null;
    [AutoNotify] private object? cmapViewModel = null;

    partial void OnConstructed()
    {
        LoadCmap();
    }
    public async void LoadCmap()
    {
        GlyphViewModel = new MultiGlyphViewModel((await Font.GetGlyphSourceAsync()));
        CmapViewModel = (await Font.GetCmapSourceAsync()).PrintCMap();
    }
}