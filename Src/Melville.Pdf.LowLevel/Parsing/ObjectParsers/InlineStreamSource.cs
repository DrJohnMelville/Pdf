using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    public class InlineStreamSource: IStreamDataSource
    {
        private long sourceFilePosition;
        private readonly ParsingFileOwner parsingFileOwner;

        public InlineStreamSource(long sourceFilePosition, ParsingFileOwner parsingFileOwner)
        {
            this.sourceFilePosition = sourceFilePosition;
            this.parsingFileOwner = parsingFileOwner;
        }

        public async ValueTask<Stream> OpenRawStream(long streamLength) =>
            await parsingFileOwner.RentStream(sourceFilePosition, streamLength);
    }
}