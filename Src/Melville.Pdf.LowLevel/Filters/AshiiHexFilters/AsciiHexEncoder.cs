using Melville.Pdf.LowLevel.Filters.Ascii85Filter;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.AshiiHexFilters
{
    public class AsciiHexEncoder:IEncoder
    {
        public byte[] Encode(byte[] data, PdfObject? parameters)
        {
            var result = new OutputBuffer(new byte[2 * data.Length]);
            for (int i = 0; i < data.Length; i++)
            {
                var (msb, lsb) = HexMath.CharPairFromByte(data[i]);
                result.Append(msb);
                result.Append(lsb);
            }

            return result.Result();
        }
    }
}