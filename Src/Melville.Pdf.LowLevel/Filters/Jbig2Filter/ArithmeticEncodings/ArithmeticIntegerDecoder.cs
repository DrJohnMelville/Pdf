using Melville.Pdf.LowLevel.Filters.Jbig2Filter.HuffmanTables;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;

public class ArithmeticIntegerDecoder: IIntegerDecoder
{
    public int GetInteger(ref BitSource source)
    {
        throw new System.NotImplementedException();
    }

    public bool HasOutOfBandRow()
    {
        throw new System.NotImplementedException();
    }

    public bool IsOutOfBand(int value)
    {
        throw new System.NotImplementedException();
    }
}