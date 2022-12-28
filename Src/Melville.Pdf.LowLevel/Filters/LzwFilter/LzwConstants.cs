namespace Melville.Pdf.LowLevel.Filters.LzwFilter;

internal static class LzwConstants
{
    public const int EndOfFileCode = 257;
    public const int ClearDictionaryCode = 256;
    public const int MaxBitLength = 12;
    public const int MaxTableSize = 1 << MaxBitLength;
}