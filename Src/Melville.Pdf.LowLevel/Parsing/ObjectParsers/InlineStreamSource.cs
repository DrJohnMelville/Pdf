using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    public class InlineStreamSource: IStreamDataSource
    {
        private long sourceFilePosition;
        private readonly ParsingFileOwner parsingFileOwner;
        private readonly IDecryptor decryptor;

        public InlineStreamSource(
            long sourceFilePosition, ParsingFileOwner parsingFileOwner, IDecryptor decryptor)
        {
            this.sourceFilePosition = sourceFilePosition;
            this.parsingFileOwner = parsingFileOwner;
            this.decryptor = decryptor;
        }

        public async ValueTask<Stream> OpenRawStream(long streamLength, PdfStream stream)
        {
            var source = await parsingFileOwner.RentStream(sourceFilePosition, streamLength);
            return await ShouldDecrypt(stream)?decryptor.WrapRawStream(source, KnownNames.StmF): source;
        }

        public Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfName? cryptFilterName) => 
            decryptor.WrapRawStream(encryptedStream, cryptFilterName);

        private async ValueTask<bool> ShouldDecrypt(PdfStream stream) => 
            await stream.GetOrNullAsync(KnownNames.Type) != KnownNames.XRef;
    }
}