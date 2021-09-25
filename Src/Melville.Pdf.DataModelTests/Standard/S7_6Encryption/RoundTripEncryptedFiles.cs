using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Document;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.ReferenceDocumentGenerator.ArgumentParsers;
using Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel.Encryption;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption
{
    public class RoundTripEncryptedFiles
    {
        private async Task TestEncryptedFile(
            CreatePdfParser gen, int V, int R, int keyLengthInBits)
        {
            var target = await gen.AsMultiBuf();
            await VerifyUserPasswordWorks(V, R, keyLengthInBits, target);
            await ParseTarget(target, PasswordType.Owner, "Owner");
        }

        private async Task VerifyUserPasswordWorks(int V, int R, int keyLengthInBits, MultiBufferStream target)
        {
            var doc = await ParseTarget(target, PasswordType.User, "User");
            var encrypt = await doc.TrailerDictionary.GetAsync<PdfDictionary>(KnownNames.Encrypt);
            await VerifyNumber(encrypt, KnownNames.V, V);
            await VerifyNumber(encrypt, KnownNames.R, R);
            await VerifyNumber(encrypt, KnownNames.Length, keyLengthInBits);
        }

        private static  Task<PdfLoadedLowLevelDocument> ParseTarget(
            MultiBufferStream target, PasswordType passwordType, string password) =>
            RandomAccessFileParser.Parse(new ParsingFileOwner(target.CreateReader(),
                new ConstantPasswordSource(passwordType, password)));

        private async ValueTask VerifyNumber(PdfDictionary encrypt, PdfName pdfName, int expected)
        {
            var num = await encrypt.GetAsync<PdfNumber>(pdfName);
            Assert.Equal(expected, num.IntValue);
            
        }

        [Fact]
        public Task V1R3Rc4() => TestEncryptedFile(new EncryptedV1Rc4(), 1,3, 40);
        [Fact]
        public Task V2R3Rc4Key128() => TestEncryptedFile(new EncryptedR3Rc4(), 2,3,128);
        [Fact]
        public Task v1R2Rc440() => TestEncryptedFile(new EncryptedR2Rc4(), 1,2,40);
        [Fact]
        public Task V2R3Rc4Key128Kb40() => TestEncryptedFile(new EncryptedV3Rc4KeyBits40(), 2,3,40);
        [Fact]
        public Task EncRefStr() => TestEncryptedFile(new EncryptedRefStm(), 2, 3, 128);
        [Fact]
        public Task SimpleV4Rc4128() => TestEncryptedFile(new Encryptedv4Rc4128(), 4, 4, 128);

        [Fact] public Task V4StreamsPlain() => TestEncryptedFile(new EncryptedV4StreamsPlain(), 4, 4, 128);
    }
}