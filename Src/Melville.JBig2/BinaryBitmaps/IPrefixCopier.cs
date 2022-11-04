using Melville.INPC;

namespace Melville.JBig2.BinaryBitmaps;

[StaticSingleton]
public sealed partial class NoTargetOffsetPrefixCopier : IBulkByteCopy
{
    public unsafe void Copy(scoped ref byte* src, scoped ref byte* dest, scoped ref BitCopier copier)
    {
        copier.Reader.Initialize(ref src);
    }
}
[StaticSingleton]
public sealed partial class EqualSourceTargetOffsetPrefixCopier : IBulkByteCopy
{
    public unsafe void Copy(scoped ref byte* src, scoped ref byte* dest, scoped ref BitCopier copier)
    {
        *dest = copier.Plan.PrefixSplicer.SplicePrefixByte(
            *dest, *src++, copier.Plan.CombinationOperator);
        dest++;
        copier.Reader = new OffsetReader(0);
    }
}
[StaticSingleton]
public sealed partial class SourceLessThanTargetOffsetPrefixCopier : IBulkByteCopy
{
    public unsafe void Copy(scoped ref byte* src, scoped ref byte* dest, scoped ref BitCopier copier)
    {
        var bitDelta = copier.Plan.FirstDestBit - copier.Plan.FirstSourceBit;
        int srcByte = *src++;
        *dest = copier.Plan.PrefixSplicer.SplicePrefixByte(
            *dest, (byte)(srcByte>>bitDelta), copier.Plan.CombinationOperator);
        dest++;
        copier.Reader = new OffsetReader((byte)(8 - bitDelta), srcByte);
    }
}
[StaticSingleton()]
public sealed partial class TargetLessThanSourceOffsetPrefixCopier : IBulkByteCopy
{
    public unsafe void Copy(scoped ref byte* src, scoped ref byte* dest, scoped ref BitCopier copier)
    {
        var bitDelta = copier.Plan.FirstSourceBit - copier.Plan.FirstDestBit;
        var srcByte = ReadTwoBytes(ref src);
        
        var bitsToCopy = (byte)(srcByte>>(8 - bitDelta));
        var splicedByte = copier.Plan.PrefixSplicer.SplicePrefixByte(
            *dest, bitsToCopy, copier.Plan.CombinationOperator);
        *dest = splicedByte;
        dest++;
        copier.Reader = new OffsetReader((byte)bitDelta, srcByte);
    }

    private static unsafe int ReadTwoBytes(ref byte* src)
    {
        int srcByte = *src++;
        srcByte <<= 8;
        srcByte |= *src++;
        return srcByte;
    }
}