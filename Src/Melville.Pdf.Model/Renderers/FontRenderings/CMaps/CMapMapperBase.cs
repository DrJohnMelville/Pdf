using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal abstract partial class CMapMapperBase
{
    [FromConstructor] private readonly VariableBitChar minValue;
    [FromConstructor] private readonly VariableBitChar maxValue;
    #if DEBUG
        partial void OnConstructed()
        {
            Debug.Assert(minValue.Length() == maxValue.Length());
        }
    #endif

    public bool AppliesTo(VariableBitChar character) => 
        minValue <= character && character <= maxValue;

    public bool Contains(CMapMapperBase other) =>
        minValue <= other.minValue && maxValue >= other.maxValue;

    protected uint OffsetFor(in VariableBitChar character) => (uint)(character - minValue);
    public int ByteLength() => minValue.Length();
    
    public abstract int WriteMapping(in VariableBitChar character, Memory<uint> target);

    public override string ToString() => $"{minValue} .. {maxValue}";

    public ByteRange MinimumContainingRange() => new(minValue, maxValue, this);
}