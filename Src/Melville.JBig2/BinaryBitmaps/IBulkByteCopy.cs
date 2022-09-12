using Melville.JBig2.Segments;

namespace Melville.JBig2.BinaryBitmaps;

public interface IBulkByteCopy
{
    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier);
}

public sealed class NullBulkByteCopy: IBulkByteCopy
{
    public static readonly IBulkByteCopy Instance = new NullBulkByteCopy();
    private NullBulkByteCopy() { }
    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
    }
}

public sealed class AlignedReplaceBulkCopy : IBulkByteCopy
{
    public static readonly IBulkByteCopy Instance = new AlignedReplaceBulkCopy();

    private AlignedReplaceBulkCopy() { }
    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
        var length = copier.Plan.WholeBytes;
        System.Runtime.CompilerServices.Unsafe.CopyBlock(dest, src, (uint)length);
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
        for (var i = copier.Plan.WholeBytes; i > 0; i--)
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
        for (var i = copier.Plan.WholeBytes; i > 0; i--)
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
        for (var i = copier.Plan.WholeBytes; i > 0; i--)
        {
            *dest = copier.Plan.CombinationOperator.Combine(*src++, *dest);
            dest++;
        }
    }
}