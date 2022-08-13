using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters;

public class FlateCodecDefinition: ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters) => 
        new(new MinimumReadSizeFilter(new FlateEncodeWrapper(data), 4));

    public async ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(2);
        int totalRead = 0;
        do
        {
            var localRead = await input.ReadAsync(buffer, 0, 2 - totalRead).CA();
            totalRead += localRead;
        } while (totalRead < 2);
        ArrayPool<byte>.Shared.Return(buffer);

        return new DeflateStream(input, CompressionMode.Decompress);
    }
}