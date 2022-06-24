namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;


public sealed class NoTargetOffsetPrefixCopier : IBulkByteCopy
{
    public static readonly NoTargetOffsetPrefixCopier Instance = new();
    private NoTargetOffsetPrefixCopier() { }
    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
        copier.Reader.Initialize(ref src);
    }
    
}
public sealed class EqualSourceTargetOffsetPrefixCopier : IBulkByteCopy
{
    public static readonly EqualSourceTargetOffsetPrefixCopier Instance = new();
    private EqualSourceTargetOffsetPrefixCopier() { }
    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
        *dest = copier.Plan.PrefixSplicer().SplicePrefixByte(
            *dest, *src++, copier.Plan.CombinationOperator);
        dest++;
    }
}
public sealed class SourceLessThanTargetOffsetPrefixCopier : IBulkByteCopy
{
    public static readonly SourceLessThanTargetOffsetPrefixCopier Instance = new();
    private SourceLessThanTargetOffsetPrefixCopier() { }
    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
        var bitDelta = copier.Plan.FirstDestBit - copier.Plan.FirstSourceBit;
        int srcByte = *src++;
        *dest = copier.Plan.PrefixSplicer().SplicePrefixByte(
            *dest, (byte)(srcByte>>bitDelta), copier.Plan.CombinationOperator);
        dest++;
        copier.Reader = new OffsetReader((byte)(8 - bitDelta), srcByte);
    }
}
public sealed class TargetLessThanSourceOffsetPrefixCopier : IBulkByteCopy
{
    public static readonly TargetLessThanSourceOffsetPrefixCopier Instance = new();
    private TargetLessThanSourceOffsetPrefixCopier() { }
    public unsafe void Copy(ref byte* src, ref byte* dest, ref BitCopier copier)
    {
        var bitDelta = copier.Plan.FirstSourceBit - copier.Plan.FirstDestBit;
        int srcByte = *src++;
        srcByte <<= 8;
        srcByte |= *src++;
        *dest = copier.Plan.PrefixSplicer().SplicePrefixByte(
            *dest, (byte)(srcByte>>bitDelta), copier.Plan.CombinationOperator);
        dest++;
        copier.Reader = new OffsetReader((byte)bitDelta, srcByte);
    }
}