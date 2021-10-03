
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Encryption;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption.S7_6_3_4PasswordAlgorithms
{
    public class SecurityHandlerFactoryTest
    {
        // test with default key length of 40
        // test v1 handler
        private readonly IPasswordSource password = new ConstantPasswordSource(PasswordType.User, new string[0]);
        
        [Theory]
        [InlineData(@"<</Encrypt <</Filter/Standard/V 2/R 3/Length 128/P -3904
            /O <E600ECC20288AD8B0D64A929C6A83EE2517679AA0218BECEEA8B7986726A8CDB>
            /U <38ACA54678D67C003A8193381B0FA1CC101112131415161718191A1B1C1D1E1F>>> 
           /ID [<1521FBE61419FCAD51878CC5D478D5FF> <1521FBE61419FCAD51878CC5D478D5FF>] >>")]
        public async Task CreateSecurityHandler(string trailer)
        { 
            await  TrailerToDocumentCryptContext.CreateDecryptorFactory(
                (PdfDictionary)await trailer.ParseObjectAsync(), password);
            // test is for absence of an exception
        }
        [Theory]
        [InlineData(@"<</Encrypt <</Filter/Standard/V 2/R 4/Length 128/P -3904
            /O <E600ECC20288AD8B0D64A929C6A83EE2517679AA0218BECEEA8B7986726A8CDB>
            /U <38ACA54678D67C003A8193381B0FA1CC101112131415161718191A1B1C1D1E1F>>> 
           /ID [<1521FBE61419FCAD51878CC5D478D5FF> <1521FBE61419FCAD51878CC5D478D5FF>] >>")]
        [InlineData(@"<</Encrypt <</Filter/Filter/V 2/R 3/Length 128/P -3904
            /O <E600ECC20288AD8B0D64A929C6A83EE2517679AA0218BECEEA8B7986726A8CDB>
            /U <38ACA54678D67C003A8193381B0FA1CC101112131415161718191A1B1C1D1E1F>>> 
           /ID [<1521FBE61419FCAD51878CC5D478D5FF> <1521FBE61419FCAD51878CC5D478D5FF>] >>")]
        [InlineData(@"<</Encrypt <</Filter/Standard/V 0/R 3/Length 128/P -3904
            /O <E600ECC20288AD8B0D64A929C6A83EE2517679AA0218BECEEA8B7986726A8CDB>
            /U <38ACA54678D67C003A8193381B0FA1CC101112131415161718191A1B1C1D1E1F>>> 
           /ID [<1521FBE61419FCAD51878CC5D478D5FF> <1521FBE61419FCAD51878CC5D478D5FF>] >>")]
        [InlineData(@"<</Encrypt <</Filter/Standard/V 3/R 2/Length 128/P -3904
            /O <E600ECC20288AD8B0D64A929C6A83EE2517679AA0218BECEEA8B7986726A8CDB>
            /U <38ACA54678D67C003A8193381B0FA1CC101112131415161718191A1B1C1D1E1F>>> 
           /ID [<1521FBE61419FCAD51878CC5D478D5FF> <1521FBE61419FCAD51878CC5D478D5FF>] >>")]
        public async Task FailToCreateSecurityHandler(string trailer)
        {
            var dict = (PdfDictionary)await trailer.ParseObjectAsync();
            await Assert.ThrowsAsync<PdfSecurityException>(
                ()=> TrailerToDocumentCryptContext.CreateDecryptorFactory(dict, password).AsTask());
        }
    }
}