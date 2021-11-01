namespace Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;

public static class RleConstants
{
    public const byte EndOfStream = 128;

    //It turns out this operation is symmetric, it will convert a length to a control byte
    // or a controlbyte to a length
    public static int RepeatedRunLength(int controlByte) => 257 - controlByte;
}