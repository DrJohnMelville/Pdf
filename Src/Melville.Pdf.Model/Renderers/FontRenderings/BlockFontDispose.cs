using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings;

internal sealed partial class BlockFontDispose : IRealizedFont
{
    [DelegateTo]
    private readonly IRealizedFont typeface;

    private BlockFontDispose(IRealizedFont typeface)
    {
        this.typeface = typeface;
    }

    public static IRealizedFont AsNonDisposableTypeface(IRealizedFont source) =>
        source is IDisposable ? new BlockFontDispose(source) : source;
}