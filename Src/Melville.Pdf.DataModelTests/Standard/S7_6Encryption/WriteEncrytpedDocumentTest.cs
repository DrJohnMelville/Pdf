using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption
{
    public class WriteEncrytpedDocumentTest
    {
        private ILowLevelDocumentCreator creator;
        private LowLevelDocumentBuilder docBuilder;
        private readonly PdfDictionary trailer;

        public WriteEncrytpedDocumentTest()
        {
            docBuilder = new LowLevelDocumentBuilder();
            docBuilder.AddToTrailerDictionary(KnownNames.ID, new PdfArray(
                new PdfString("12345678901234567890123456789012"),
                new PdfString("12345678901234567890123456789012")));
            docBuilder.AddEncryption(new DocumentEncryptorV3Rc4128("User", "Owner", PdfPermission.None));
            trailer = docBuilder.CreateTrailerDictionary();
            creator = new LowLevelDocumentCreator(docBuilder);
        }
        private async Task<string> Write(PdfLowLevelDocument doc)
        {
            var target = new MultiBufferStream();
            var writer = new LowLevelDocumentWriter(PipeWriter.Create(target), doc, "User");
            await writer.WriteAsync();
            return target.CreateReader().ReadToArray().ExtendedAsciiString();
        }

        [Fact]
        public async Task WriteRC4V3EncryptedDocument()
        {
            docBuilder.Add(new PdfString("Encrypted String"));
            docBuilder.Add(docBuilder.NewStream("This is an encrypted stream"));
            var doc = creator.CreateDocument();
            var str = await Write(doc);
            Assert.DoesNotContain("Encrypted String", str);
            Assert.DoesNotContain("encrypted stream", str);
           
            var doc2 = await str.ParseWithPassword("User", PasswordType.User);
            var outstr = await doc2.Objects[(1,0)].DirectValue();
            Assert.Equal("Encrypted String", outstr.ToString());
            
        }

        [Fact] 
        public void EcryptionRequiresAnID()
        {
            Assert.True(trailer.ContainsKey(KnownNames.ID));
        }

        [Fact] 
        public async Task EcryptionWithV3Dictionary()
        {
            Assert.True(trailer.ContainsKey(KnownNames.Encrypt));
            var dict = await trailer.GetAsync<PdfDictionary>(KnownNames.Encrypt);
            Assert.Equal(2, (await dict.GetAsync<PdfNumber>(KnownNames.V)).IntValue);
            Assert.Equal(3, (await dict.GetAsync<PdfNumber>(KnownNames.R)).IntValue);
            Assert.Equal(128, (await dict.GetAsync<PdfNumber>(KnownNames.Length)).IntValue);
            Assert.Equal(-1, (await dict.GetAsync<PdfNumber>(KnownNames.P)).IntValue);
            Assert.Equal(32, (await dict.GetAsync<PdfString>(KnownNames.U)).Bytes.Length);
            Assert.Equal(32, (await dict.GetAsync<PdfString>(KnownNames.O)).Bytes.Length);
            Assert.Equal(KnownNames.Standard, (await dict.GetAsync<PdfName>(KnownNames.Filter)));
            
        }
    }
}