using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.LowLevel.Model.Objects;

public static class StreamAsTextStream
{
    public static async ValueTask<TextReader> TextStreamReader(this PdfStream source)
    {
        var stream = await source.StreamContentAsync().ConfigureAwait(false);
        var buffer = new byte[2];
        var len=await buffer.FillBufferAsync(0, 2, stream).ConfigureAwait(false);
        return Utf16BE.HasUtf16BOM(buffer) ? 
            new StreamReader(stream, Utf16BE.UtfEncoding, false, -1, false) : 
            new StreamReader(PushbackPrefix(len, buffer, stream), new PdfDocEncoding(), false, -1, false);
    }

    private static ConcatStream PushbackPrefix(int len, byte[] buffer, Stream stream) => 
        new(MakePrefixStream(len, buffer), stream);

    private static MemoryStream MakePrefixStream(int len, byte[] buffer) => 
        new(TryTrimBuffer(len, buffer));

    private static byte[] TryTrimBuffer(int len, byte[] buffer) => 
        len < 2 ? buffer[..len]:buffer;
        
}