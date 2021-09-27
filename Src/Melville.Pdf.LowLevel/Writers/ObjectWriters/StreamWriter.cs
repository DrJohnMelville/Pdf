using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters
{
    public static class StreamWriter
    {
        private static byte[] streamToken = {32, 115, 116, 114, 101, 97, 109, 13, 10}; //  stream\r\n
        private static byte[] endStreamToken = 
            {13, 10, 101, 110, 100, 115, 116, 114, 101, 97, 109}; //  \r\nendstream
        public static async ValueTask<FlushResult> Write(
            PipeWriter target, PdfObjectWriter innerWriter, PdfStream item,
            IObjectEncryptor encryptor)
        {
            await DictionaryWriter.Write(target, innerWriter, item.RawItems);
            target.WriteBytes(streamToken);
            var rawStream = await item.GetEncodedStreamAsync();
            await using var encryptedStream =
                (await item.HasFilterOfType(KnownNames.Crypt).ConfigureAwait(false))?
                    rawStream: 
                    encryptor.WrapReadingStreamWithEncryption(rawStream);
            await encryptedStream.CopyToAsync(target);
            target.WriteBytes(endStreamToken);
            return await target.FlushAsync();
        }
    }
}