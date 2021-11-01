using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

public static class StreamWriter
{
    private static byte[] streamToken = {32, 115, 116, 114, 101, 97, 109, 13, 10}; //  stream\r\n
    private static byte[] endStreamToken = 
        {13, 10, 101, 110, 100, 115, 116, 114, 101, 97, 109}; //  \r\nendstream
    public static async ValueTask<FlushResult> Write(
        PipeWriter target, PdfObjectWriter innerWriter, PdfStream item,
        IObjectCryptContext encryptor)
    {
        Stream diskrep;
        await using var rawStream = await item.StreamContentAsync(StreamFormat.DiskRepresentation, encryptor);
        diskrep = await EnsureStreamHasKnownLength(rawStream);
            
        await DictionaryWriter.Write(target, innerWriter, 
            item.MergeItems((KnownNames.Length, new PdfInteger(diskrep.Length))));
        target.WriteBytes(streamToken);
        await diskrep.CopyToAsync(target);
        target.WriteBytes(endStreamToken);
        return await target.FlushAsync();
    }

    private static async Task<Stream> EnsureStreamHasKnownLength(Stream rawStream)
    {
        Stream diskrep;
        if (rawStream.Length < 1)
        {
            var mbs = new MultiBufferStream(2048);
            await rawStream.CopyToAsync(mbs);
            diskrep = mbs.CreateReader();
        }
        else
        {
            diskrep = rawStream;
        }

        return diskrep;
    }
}