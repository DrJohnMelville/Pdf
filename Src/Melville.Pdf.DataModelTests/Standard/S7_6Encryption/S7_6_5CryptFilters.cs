using System;
using System.Linq;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.LowLevel.Writers.DocumentWriters;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;
using Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel.Encryption;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption
{
    public class S7_6_5CryptFilters
    {
        private static async Task VerifyStringAndStreamEncoding(bool hideStream, bool hideString,
            LowLevelDocumentCreator? creator)
        {
            creator.Add(new PdfString("plaintext string"));
            creator.Add(creator.NewStream("plaintext stream"));
            var str = await creator.AsStringAsync();
            Assert.Equal(!hideString, str.Contains("plaintext string"));
            Assert.Equal(!hideStream, str.Contains("plaintext stream"));
            var doc = await str.ParseDocumentAsync();
            Assert.Equal(4, doc.Objects.Count);
            Assert.Equal("plaintext string", (await doc.Objects[(2, 0)].DirectValue()).ToString());
            Assert.Equal("plaintext stream", await (
                    await ((PdfStream)(
                        await doc.Objects[(3, 0)].DirectValue().ConfigureAwait(false))).GetDecodedStreamAsync())
                .ReadAsStringAsync());
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(true, false)]
        [InlineData(false, true)]
        [InlineData(false, false)]
        public async Task CanUseDefaultStringsIn(bool hideStream, bool hideString)
        {
            var creator = new LowLevelDocumentCreator();
            creator.AddEncryption(DocumentEncryptorFactory.V4("","", PdfPermission.None,
                Encoder(hideStream), Encoder(hideString), new V4CfDictionary(KnownNames.V2, 16)));
            await VerifyStringAndStreamEncoding(hideStream, hideString, creator);
        }

        [Fact]
        public async Task NoneCrptFilterWorks()
        {
            var creator = new LowLevelDocumentCreator();
            creator.AddEncryption(DocumentEncryptorFactory.V4("","", PdfPermission.None,
                Encoder(true), Encoder(true), new V4CfDictionary(KnownNames.None, 16)));
            await VerifyStringAndStreamEncoding(false, false, creator);
            
        }
        
        private static PdfName Encoder(bool hideString)
        {
            return hideString?KnownNames.StdCF:KnownNames.Identity;
        }
    }
}