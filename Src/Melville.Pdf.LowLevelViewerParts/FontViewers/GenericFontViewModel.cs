using Melville.Fonts;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class GenericFontViewModel
{
    [FromConstructor] public IGenericFont Font { get; }
    public string Title => "Generic Font View";
    public string? ToolTip => null;
    [AutoNotify] private string fontName = "";
    [AutoNotify] private object? glyphViewModel = null;
    [AutoNotify] private object? cmapViewModel = null;
    [AutoNotify] private object? glyphNames = null; 

    partial void OnConstructed()
    {
        LoadCmap();
    }
    public async void LoadCmap()
    {
        fontName = await Font.FontFamilyNameAsync();
        GlyphViewModel = new MultiGlyphViewModel((await Font.GetGlyphSourceAsync()));
        CmapViewModel = (await Font.GetCmapSourceAsync()).PrintCMap();
        GlyphNames = new MultiStringViewModel(LoadGlyphsAsync, "Glyph Names");
    }

    private async ValueTask<IReadOnlyList<string>> LoadGlyphsAsync() =>
        (await Font.GlyphNamesAsync())
        .Select((item, i) => $"0x{i:X} => {item}")
        .ToArray();
}