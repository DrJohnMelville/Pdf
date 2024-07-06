using Melville.Fonts.SfntParsers;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers.SfntViews;

public class SfntViewModel : SingleFontViewModel
{
    public SfntViewModel(SFnt font) : base(font)
    {
        Tables = [.. Tables, .. font.Tables.Select(i => new TableViewModel(font, i))];
    }
}