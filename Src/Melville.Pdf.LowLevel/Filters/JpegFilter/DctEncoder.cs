using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.JpegLibrary.PipeAmdStreamAdapters;
using Melville.Pdf.LowLevel.Encryption.EncryptionKeyAlgorithms;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Memory;

namespace Melville.Pdf.LowLevel.Filters.JpegFilter;

internal class DctCodec : ICodecDefinition
{
    public ValueTask<IPdfStreamReader> EncodeOnReadStreamAsync(
        IPdfStreamReader data, PdfDirectObject parameters, object? context) => 
        throw new NotSupportedException();

    public async ValueTask<IPdfStreamReader> DecodeOnReadStreamAsync(
        IPdfStreamReader input, PdfDirectObject parameters, object? context)
    {
        var (bytes, length) =  await new JpegStreamFactory(
            await new DctDecodeParameters(parameters).ColorTransformAsync().CA())
                    .FromStreamAsync(input).CA();
        return new RentedMemoryReader(bytes, length);
    }

    private class RentedMemoryReader(byte[] data, int length) : MemoryReader(data.AsMemory(0, length)),
        IPdfStreamReader
    {
        public void Dispose() => ArrayPool<byte>.Shared.Return(data);
    }

}