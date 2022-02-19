namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public record struct CcittCode(CcittCodeOperation Operation, ushort Length)
{
   public int VerticalOffset => Length - 3;
}