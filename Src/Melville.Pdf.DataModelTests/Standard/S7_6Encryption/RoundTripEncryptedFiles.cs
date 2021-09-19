using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;
using Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel.Encryption;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption
{
    public class RoundTripEncryptedFiles
    {
        private static async Task TestEncryptedFile(CreatePdfParser gen)
        {
            var target = new MultiBufferStream();
            await gen.WritePdfAsync(target);
            var ps = new ParsingFileOwner(target.CreateReader(),
                new ConstantPasswordSource(PasswordType.User, "User"));
            var doc = await RandomAccessFileParser.Parse(ps);
            // for right now just parsing without errors is enough
        }

        [Fact]
        public Task V3Rc4Key128() => TestEncryptedFile(new EncryptedV3Rc4());
        [Fact]
        public Task EncRefStr() => TestEncryptedFile(new EncryptedRefStm());
    }
}