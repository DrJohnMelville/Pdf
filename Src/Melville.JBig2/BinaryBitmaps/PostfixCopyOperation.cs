namespace Melville.JBig2.BinaryBitmaps;

internal class PostfixCopyOperation : IBulkByteCopy
{
    public static readonly IBulkByteCopy Instance = new PostfixCopyOperation();

    private PostfixCopyOperation() { }
    public unsafe void Copy(scoped ref byte* src, scoped ref byte* dest, scoped ref BitCopier copier)
    {
        var postSplicer = copier.Plan.PostSplicer;
        var lastSource = copier.Reader.ReadBye(ref src);
        *dest = postSplicer.SplicePostFixByte(lastSource, *dest, copier.Plan.CombinationOperator);
    }
}