using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.StreamFilters;

namespace Melville.Pdf.LowLevel.Model.Objects;

/// <summary>
/// This helped method interprets a text stream by reading it's byte order mark.
/// </summary>
public static class StreamAsTextStream
{
    /// <summary>
    /// Intepret a PdfStream as a text stream and create the approprate TextReader by examining the
    /// byte order mark.
    /// </summary>
    /// <param name="source">The source PDF string.</param>
    /// <returns>A text reader that will properly decode the text stream.</returns>
    public static async ValueTask<TextReader> TextStreamReader(this PdfStream source)
    {
        var stream = await source.StreamContentAsync().CA();
        var buffer = new byte[3];
        var len=await buffer.FillBufferAsync(0, buffer.Length, stream).CA();
        var (encoder, bomLength) = ByteOrderDetector.DetectByteOrder(buffer);
        return
            new StreamReader(TrySkipByteOrderMark(stream, len, buffer, bomLength),
                encoder, false, -1, false);
    }

    private static Stream TrySkipByteOrderMark(Stream stream, int len, byte[] buffer,
        int bomLength) => 
        bomLength == len ? stream : PushbackPrefix(len, bomLength, buffer, stream);

    private static ConcatStream PushbackPrefix(int len, int bomLength, byte[] buffer, Stream stream) => 
        new(MakePrefixStream(len, bomLength, buffer), stream);

    private static MemoryStream MakePrefixStream(int len, int bomLength, byte[] buffer) => 
        new(TryTrimBuffer(len, bomLength, buffer));

    private static byte[] TryTrimBuffer(int len, int bomLength, byte[] buffer)
    {
        Debug.Assert(bomLength < len);
        return len < 2 ? buffer[bomLength..len] : buffer;
    }
}