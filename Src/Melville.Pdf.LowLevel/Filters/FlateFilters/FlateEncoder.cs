using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters;

internal class FlateCodecDefinition: ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters) => 
        new(new MinimumReadSizeFilter(new FlateEncodeWrapper(data), 4));

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters)
    {
        await Skip2BytePrefixAsync(input);
        return new DeflateStream(input, CompressionMode.Decompress);
    }

    private static readonly byte[] buffer = new byte[2];
    private static async Task Skip2BytePrefixAsync(Stream input)
    {
        await buffer.FillBufferAsync(0, 2, input).CA();
        Debug.Assert((buffer[0] & 0x0F) == 0x08);
        Debug.Assert(((256 * buffer[0])+buffer[1]) % 31 == 0);
    }
}