using Melville.Fonts;
using Melville.Fonts.SfntParsers;
using Melville.Fonts.Type1TextParsers;
using Melville.Pdf.LowLevelViewerParts.FontViewers.SfntViews;
using Melville.Pdf.LowLevelViewerParts.FontViewers.Type1Views;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers
{
    public static class SpecificFontViewModelFactory
    {
        public static object? CreateSpecificViewModel(this IGenericFont? font) => font switch
        {
            SFnt ft => new SfntViewModel(ft),
            Type1GenericFont t1 => new Type1ViewModel(t1),
            { } x => new SingleFontViewModel(x),
            _ => null
        };
    }
}