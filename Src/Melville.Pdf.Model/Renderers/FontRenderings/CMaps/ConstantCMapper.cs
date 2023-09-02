using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal partial class ConstantCMapper : CMapMapperBase
{
    [FromConstructor] private readonly uint constantValue;

    public override int WriteMapping(in VariableBitChar offset, Memory<uint> target)
    {
        if (target.Length < 1) return -1;
        target.Span[0] = constantValue;
        return 1;
    }
}