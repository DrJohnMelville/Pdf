using System;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public ref struct BitCopier
{
    public OffsetReader Reader;
    public readonly RowCopyPlan Plan;
    private IPrefixCopier prefixCopier;
    private IBulkByteCopy bulkCopier;
    private ByteSplicer postSplicer;

    public BitCopier(in RowCopyPlan plan)
    {
        Reader = new OffsetReader(plan.FirstSourceBit);
        Plan = plan;
        prefixCopier = plan.PrefixCopier();
        bulkCopier = plan.BulkCopier();
        postSplicer = plan.PostSplicer();
    }

    public unsafe void Copy(in Span<byte> src, in Span<byte> dest)
    {
        fixed(byte* srcPointer = src)
        fixed (byte* destPointer = dest)
        {
            Copy(srcPointer, destPointer);
        }
    }

    public unsafe void Copy(byte* src, byte* dest)
    {
        prefixCopier.CopyPrefix(ref src, ref dest, ref this);
        bulkCopier.Copy(ref src, ref dest, ref this);
        TryCopySuffix(src, dest);
    }

    private unsafe void TryCopySuffix(byte* src, byte* dest)
    {
        if (HasSuffiBits())
        {
            var lastSource = Reader.ReadBye(ref src);
            *dest = postSplicer.SplicePostFixByte(lastSource, *dest, Plan.CombinationOperator);
        }
    }
    private bool HasSuffiBits() => Plan.SuffixBits > 0;
}