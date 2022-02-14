using System;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public ref struct BitTarget
{
    private readonly Span<byte> target;
    private readonly BitWriter writer;
    public int BytesWritten { get; set; }

    public BitTarget(Span<byte> target, BitWriter writer)
    {
        this.target = target;
        this.writer = writer;
        BytesWritten = 0;
    }

    public bool TryWriteBits(uint data, int bits)
    {
        if (BytesWritten + BytesNeeded(bits) >= target.Length) return false;
        BytesWritten += writer.WriteBits(data, bits, target[BytesWritten..]);
        return true;
    }
    private int BytesNeeded(int bits) => (7+bits)/8;
        
}