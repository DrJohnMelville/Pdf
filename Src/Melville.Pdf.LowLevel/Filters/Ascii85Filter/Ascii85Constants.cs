namespace Melville.Pdf.LowLevel.Filters.Ascii85Filter;

internal static class Ascii85Constants
{
    public const byte FirstChar = (byte)'!';
    public const byte IncompleteGroupPadding = (byte)'u';
    public const byte FirstTerminatingChar = (byte)'~';
    public const byte SecondTerminatingChar = (byte)'>';
}