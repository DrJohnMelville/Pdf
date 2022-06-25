namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

public class PostfixCopyOperation : IBulkByteCopy
{
    public static readonly IBulkByteCopy Instance = new PostfixCopyOperation();

    private PostfixCopyOperation() { }
    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
        var postSplicer = copier.Plan.PostSplicer;
        var lastSource = copier.Reader.ReadBye(ref src);
        *dest = postSplicer.SplicePostFixByte(lastSource, *dest, copier.Plan.CombinationOperator);
    }
}