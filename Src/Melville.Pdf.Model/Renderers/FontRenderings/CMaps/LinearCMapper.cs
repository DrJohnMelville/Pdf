using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal partial class LinearCMapper : CMapMapperBase
{
    [FromConstructor] private readonly uint constantValue;

    public override int WriteMapping(in VariableBitChar character, Span<uint> target)
    {
        if (target.Length < 1) return -1;
        var offsetFor = OffsetFor(character);
        target[0] = constantValue + offsetFor;
        return 1;
    }
}