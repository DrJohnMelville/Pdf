using System.IO;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;

namespace Melville.Pdf.LowLevel.Model.Objects.StreamParts;

internal class LiteralStreamSource(MultiBufferStreamSource source, StreamFormat sourceFormat) : IStreamDataSource
{
    public StreamFormat SourceFormat { get; } = sourceFormat;

    public Stream OpenRawStream(long streamLength) => source.Stream;

    public Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfDirectObject cryptFilterName) =>
        encryptedStream;
    public Stream WrapStreamWithDecryptor(Stream encryptedStream) =>
        encryptedStream;
}