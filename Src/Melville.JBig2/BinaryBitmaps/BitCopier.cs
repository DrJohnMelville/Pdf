using System;
using Melville.INPC;

namespace Melville.JBig2.BinaryBitmaps;

public ref partial struct BitCopier
{
    [FromConstructor]public OffsetReader Reader;
    [FromConstructor]public readonly RowCopyPlan Plan;
    [FromConstructor]private readonly IBulkByteCopy prefixCopier;
    [FromConstructor]private readonly IBulkByteCopy bulkCopier;
    [FromConstructor]private readonly IBulkByteCopy postfixCopier;

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

}