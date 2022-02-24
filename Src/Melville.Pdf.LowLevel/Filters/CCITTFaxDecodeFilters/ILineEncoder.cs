namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public interface ILineEncoder
{
    bool TryWriteLinePrefix(ref CcittBitWriter writer, ref int a0, in LinePair lines);
    bool TryWriteRun(ref CcittBitWriter writer, ref int a0, in LinePair lines);
}