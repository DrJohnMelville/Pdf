using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.AshiiHexFilters
{
    public class AsciiHexCompressor:ICompressor
    {
        public byte[] Compress(byte[] data, PdfObject? parameters)
        {
            var result = new byte[2 * data.Length];
            var position = 0;
            for (int i = 0; i < data.Length; i++)
            {
                var (msb, lsb) = HexMath.CharPairFromByte(data[i]);
                result[position++] = msb;
                result[position++] = lsb;
            }

            return result;
        }
    }
}