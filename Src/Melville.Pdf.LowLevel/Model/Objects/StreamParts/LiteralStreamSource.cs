using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Model.Objects.StreamParts;

internal class LiteralStreamSource : IStreamDataSource
{
    private MultiBufferStream source;
    public StreamFormat SourceFormat { get; }
    public LiteralStreamSource(string data, StreamFormat sourceFormat) :
        this(data.AsExtendedAsciiBytes(), sourceFormat)
    {
    }
    public LiteralStreamSource(byte[] buffer, StreamFormat sourceFormat) :
        this(new MultiBufferStream(buffer), sourceFormat)
    {
    }
    public LiteralStreamSource(MultiBufferStream stream, StreamFormat sourceFormat)
    {
        source = stream;
        SourceFormat = sourceFormat;
    }

    public Stream OpenRawStream(long streamLength) => source.CreateReader();

    public Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfDirectObject cryptFilterName) =>
        encryptedStream;
    public Stream WrapStreamWithDecryptor(Stream encryptedStream) =>
        encryptedStream;
}