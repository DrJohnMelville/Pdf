using System.ComponentModel.Design;
using Melville.Fonts;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class SingleFontViewModel(IGenericFont font)
{
    [AutoNotify] private IReadOnlyList<object> tables =
        [new GenericFontViewModel(font, null, "Generic Font View")];
}