using Melville.Fonts;
using Melville.INPC;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class SingleFontViewModel
{
    public IReadOnlyList<object> Tables { get; protected set; } = [];

    public SingleFontViewModel(IGenericFont font)
    {
        Tables = [new GenericFontViewModel(font)];
    }
}

public partial class GenericFontViewModel
{
    [FromConstructor] public IGenericFont Font { get; }
}