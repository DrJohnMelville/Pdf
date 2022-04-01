using System.IO;
using System.Text;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Objects.StringEncodings;

namespace Melville.Pdf.LowLevel.Model.Objects;

public static class StreamAsTextStream
{
    public static async ValueTask<TextReader> TextStreamReader(this PdfStream source)
    {
        var stream = await source.StreamContentAsync().CA();
        var buffer = new byte[2];
        var len=await buffer.FillBufferAsync(0, 2, stream).CA();
        var encoder = ByteOrderDetector.DetectByteOrder(buffer);
        return
            new StreamReader(TrySkipByteOrderMark(encoder, stream, len, buffer),
                encoder, false, -1, false);
    }

    private static Stream TrySkipByteOrderMark(Encoding encoder, Stream stream, int len, byte[] buffer) => 
        encoder is UnicodeEncoding ? stream : PushbackPrefix(len, buffer, stream);

    private static ConcatStream PushbackPrefix(int len, byte[] buffer, Stream stream) => 
        new(MakePrefixStream(len, buffer), stream);

    private static MemoryStream MakePrefixStream(int len, byte[] buffer) => 
        new(TryTrimBuffer(len, buffer));

    private static byte[] TryTrimBuffer(int len, byte[] buffer) => 
        len < 2 ? buffer[..len]:buffer;
        
}