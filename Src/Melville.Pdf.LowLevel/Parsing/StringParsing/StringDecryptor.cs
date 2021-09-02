using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.StringParsing
{
    public class StringDecryptor : IPdfObjectParser
    {
        private readonly IPdfObjectParser inner;

        public StringDecryptor(IPdfObjectParser inner)
        {
            this.inner = inner;
        }

        public async Task<PdfObject> ParseAsync(IParsingReader source)
        {
            var ret = await inner.ParseAsync(source);
            if (ret is PdfString pdfString)
            {
                source.Decryptor.DecryptStringInPlace(pdfString.Bytes);
            }

            return ret;
        }
    }
}