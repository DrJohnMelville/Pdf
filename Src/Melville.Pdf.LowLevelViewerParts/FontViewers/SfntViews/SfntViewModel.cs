using Melville.Fonts.SfntParsers;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.SfntViews;

public class SfntViewModel : SingleFontViewModel
{
    public SfntViewModel(SFnt font) : base(font)
    {
        Tables = [.. Tables, 
            .. font.Tables.Select(i => new TableViewModel(font, i))];
        TryReadInnerTables(font);
    }

    private async void TryReadInnerTables(SFnt font)
    {
        var inner = await font.InnerGenericFontsAsync();
        if (inner.Count is 0) return;

        Tables = [.. Tables,
            .. inner.Select(i => new GenericFontViewModel(i, "Inner Generic Font"))];
    }
}