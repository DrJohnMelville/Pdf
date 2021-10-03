using System.IO;
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
    public class EncryptionAlgorithmsActuallyEncryptTest
    {
        
        private async Task<string> Write(PdfLowLevelDocument doc)
        {
            var target = new MultiBufferStream();
            var writer = new LowLevelDocumentWriter(PipeWriter.Create(target), doc, "User");
            await writer.WriteAsync();
            return target.CreateReader().ReadToArray().ExtendedAsciiString();
        }

        [Fact]
        public Task AesLength128() => 
            CreateAndTestDocument(DocumentEncryptorFactory.V4("User", "Owner", PdfPermission.None, KnownNames.AESV2, 8));
        [Fact]
        public Task Rc4Length128() => 
            CreateAndTestDocument(DocumentEncryptorFactory.V2R3Rc4128("User", "Owner", PdfPermission.None));
        [Fact]
        public Task Rc4Length40() => 
            CreateAndTestDocument(DocumentEncryptorFactory.v1R2Rc440("User", "Owner", PdfPermission.None));

        private async Task CreateAndTestDocument(ILowLevelDocumentEncryptor encryptionDeclaration)
        {
            var docBuilder = new LowLevelDocumentBuilder();
            docBuilder.AddToTrailerDictionary(KnownNames.ID, new PdfArray(
                PdfString.CreateAscii("12345678901234567890123456789012"),
                PdfString.CreateAscii("12345678901234567890123456789012")));
            docBuilder.AddEncryption(encryptionDeclaration);
            var creator = new LowLevelDocumentCreator(docBuilder);

            docBuilder.Add(PdfString.CreateAscii("Encrypted String"));
            docBuilder.Add(docBuilder.NewStream("This is an encrypted stream"));
            var doc = creator.CreateDocument();
            var str = await Write(doc);
            Assert.DoesNotContain("Encrypted String", str);
            Assert.DoesNotContain("encrypted stream", str);

            var doc2 = await str.ParseWithPassword("User", PasswordType.User);
            var outstr = await doc2.Objects[(2, 0)].DirectValueAsync();
            Assert.Equal("Encrypted String", outstr.ToString());
            var stream = (PdfStream)await doc2.Objects[(3,0)].DirectValueAsync();
            Assert.Equal("This is an encrypted stream", await new StreamReader(
                await stream.StreamContentAsync()).ReadToEndAsync());
            
        }
    }
}