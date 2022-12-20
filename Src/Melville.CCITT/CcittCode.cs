namespace Melville.CCITT;

internal record struct CcittCode(CcittCodeOperation Operation, ushort Length)
{
   public int VerticalOffset => Length - 3;
}