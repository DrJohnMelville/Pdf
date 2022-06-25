using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public class SingleByteCopy : IBulkByteCopy
{
    public static readonly IBulkByteCopy Instance = new SingleByteCopy();
    protected SingleByteCopy() { }

    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
        copier.Reader.Initialize(ref src);
        var source = copier.Reader.ReadBye(ref src);
        var bitlen = copier.Plan.SuffixBits;
        source >>= 8 - bitlen; // low x bits 0f source are the data
        var destOffset = (8 - bitlen) - copier.Plan.FirstDestBit;
        source <<= destOffset;
        var mask = ((1 << bitlen) - 1) << destOffset;
        var finalByte = (*dest) & mask;
        finalByte = copier.Plan.CombinationOperator.Combine(finalByte, source);
        *dest =(byte) ((*dest & ~mask) | finalByte);
    }
}