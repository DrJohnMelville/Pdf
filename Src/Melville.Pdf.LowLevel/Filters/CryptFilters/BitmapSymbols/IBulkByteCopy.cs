
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.Segments;

namespace Melville.Pdf.LowLevel.Filters.CryptFilters.BitmapSymbols;

public interface IBulkByteCopy
{
    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier);
}

public sealed class AlignedReplaceBulkCopy : IBulkByteCopy
{
    public static readonly IBulkByteCopy Instance = new AlignedReplaceBulkCopy();

    private AlignedReplaceBulkCopy() { }
    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
        var length = copier.Plan.WholeBytes;
        System.Runtime.CompilerServices.Unsafe.CopyBlock(dest, src, length);
        src += length;
        dest += length;
    }
}
public sealed class SourceOffsetBulkCopy : IBulkByteCopy
{
    public static readonly IBulkByteCopy Instance = new SourceOffsetBulkCopy();
    private SourceOffsetBulkCopy() { }

    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
        var srcEnd = src + copier.Plan.WholeBytes;
        while (src < srcEnd)
        {
            *dest++ = copier.Reader.ReadBye(ref src);
        }
    }
}
public sealed class SourceOffsetBulkOperation : IBulkByteCopy
{
    public static readonly IBulkByteCopy Instance = new SourceOffsetBulkOperation();
    private SourceOffsetBulkOperation() { }

    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
        var srcEnd = src + copier.Plan.WholeBytes;
        while (src < srcEnd)
        {
            *dest = copier.Plan.CombinationOperator.Combine(copier.Reader.ReadBye(ref src), *dest);
            dest++;
        }
    }
}
public sealed class AlignedBulkOperation : IBulkByteCopy
{
    public static readonly IBulkByteCopy Instance = new AlignedBulkOperation();
    private AlignedBulkOperation() { }

    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
        var srcEnd = src + copier.Plan.WholeBytes;
        while (src < srcEnd)
        {
            *dest = copier.Plan.CombinationOperator.Combine(*src++, *dest);
            dest++;
        }
    }
}