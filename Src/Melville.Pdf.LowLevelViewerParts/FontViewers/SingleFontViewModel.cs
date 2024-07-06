using System.ComponentModel.Design;
using Melville.Fonts;

namespace Melville.Pdf.LowLevelViewerParts.FontViewers;

public partial class SingleFontViewModel(IGenericFont font)
{
    public IReadOnlyList<object> Tables { get; protected set; } = 
        [new GenericFontViewModel(font)];
}