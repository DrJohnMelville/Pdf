using System.IO;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;

namespace Melville.Pdf.DataModelTests.StreamUtilities;

public static class StreamTest
{
    public static async Task TestContentAsync(
        string encoded, string decoded, PdfDirectObject decoder, PdfDirectObject parameters)
    {
        using var streamToRead = await StaticCodecFactory.CodecFor(decoder)
                        .DecodeOnReadStreamAsync(StringAsAsciiStream(encoded), parameters, null);
        await VerifyStreamContentAsync(decoded,
                    streamToRead);
    }

    public static async Task VerifyStreamContentAsync(string src, IAsyncReader streamToRead)
    {
        using var pb = new ParsingBuffer(streamToRead);
        while (!pb.DoneReadingSource) await pb.ReadBytesAsync();
        var str = pb.Peek().ExtendedAsciiString();
        Assert.Equal(src, str);
    }
    private static IPdfStreamReader StringAsAsciiStream(string content) => 
        new PdfMemoryReader(ExtendedAsciiEncoding.AsExtendedAsciiBytes(content));
}