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
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectObject parameters, object? context) => 
        new(new MinimumReadSizeFilter(new FlateEncodeWrapper(data), 4));

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectObject parameters, object? context)
    {
        await Skip2BytePrefixAsync(input);
        return new DeflateStream(input, CompressionMode.Decompress);
    }

    private static readonly byte[] buffer = new byte[2];
    private static Task Skip2BytePrefixAsync(Stream input)
    {
        return buffer.FillBufferAsync(0, 2, input);
    }
}