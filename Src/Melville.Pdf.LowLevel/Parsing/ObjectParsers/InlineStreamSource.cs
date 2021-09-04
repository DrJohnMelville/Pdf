using System.IO;
using System.Threading.Tasks;
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

        public async ValueTask<Stream> OpenRawStream(long streamLength) =>
            decryptor.WrapRawStream(await parsingFileOwner.RentStream(sourceFilePosition, streamLength));
    }
}