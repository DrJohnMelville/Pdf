using System;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public ref struct BitCopier
{
    public OffsetReader Reader;
    public readonly RowCopyPlan Plan;
    private readonly IBulkByteCopy prefixCopier;
    private readonly IBulkByteCopy bulkCopier;
    private readonly IBulkByteCopy postfixCopier;

    public BitCopier(in RowCopyPlan plan)
    {
        Reader = new OffsetReader(plan.FirstSourceBit);
        Plan = plan;
        prefixCopier = plan.PrefixCopier();
        bulkCopier = plan.BulkCopier();
        postfixCopier = plan.PostfixCopier();
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
        prefixCopier.Copy(ref src, ref dest, ref this);
        bulkCopier.Copy(ref src, ref dest, ref this);
        postfixCopier.Copy(ref src, ref dest, ref this);
    }

    // private unsafe void TryCopySuffix(byte* src, byte* dest)
    // {
    //     if (HasSuffiBits())
    //     {
    //         var lastSource = Reader.ReadBye(ref src);
    //         *dest = postSplicer.SplicePostFixByte(lastSource, *dest, Plan.CombinationOperator);
    //     }
    // }
//    private bool HasSuffiBits() => Plan.SuffixBits > 0;
}