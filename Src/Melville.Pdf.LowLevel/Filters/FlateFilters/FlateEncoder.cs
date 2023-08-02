using System.Buffers;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FlateFilters;

internal class FlateCodecDefinition: ICodecDefinition
{
    public ValueTask<Stream> EncodeOnReadStreamAsync(Stream data, PdfDirectValue parameters) => 
        new(new MinimumReadSizeFilter(new FlateEncodeWrapper(data), 4));

    public async ValueTask<Stream> DecodeOnReadStreamAsync(Stream input, PdfDirectValue parameters)
    {
        await Skip2BytePrefixAsync(input);
        return new DeflateStream(input, CompressionMode.Decompress);
    }

    private static async Task Skip2BytePrefixAsync(Stream input)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(2);
        int totalRead = 0, localRead;
        do
        {
            localRead = await input.ReadAsync(buffer, 0, 2 - totalRead).CA();
            totalRead += localRead;
        } while (NotAtEndOfPrefix(totalRead) && NotAtEndOfStream(localRead));

        ArrayPool<byte>.Shared.Return(buffer);
    }

    private static bool NotAtEndOfPrefix(int totalRead) => totalRead < 2;
    private static bool NotAtEndOfStream(int localRead) => localRead > 0;
}