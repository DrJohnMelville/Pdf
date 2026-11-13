using System.IO;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using System;
using Melville.StreamInterfaces.Memory;
using Melville.StreamInterfaces;

namespace Melville.Pdf.LowLevel.Model.Objects.StreamParts;

internal class LiteralStreamSource(MultiBufferStreamSource source, StreamFormat sourceFormat) : IStreamDataSource
{
    public StreamFormat SourceFormat { get; } = sourceFormat;

    // this needs to ignore the streamLength parameter or else the writers do not work.
    public IPdfStreamReader OpenRawReader(long streamLength) =>
        new PdfMemoryReader(source.Source.AsMemory());

    public Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfDirectObject cryptFilterName) =>
        encryptedStream;
    public Stream WrapStreamWithDecryptor(Stream encryptedStream) =>
        encryptedStream;
}

internal class PdfMemoryReader(Memory<byte> source) : MemoryReader(source), IPdfStreamReader
{
    public void Dispose()
    {
        // we do not have and disposal, we just need to implement the interface
    }
}