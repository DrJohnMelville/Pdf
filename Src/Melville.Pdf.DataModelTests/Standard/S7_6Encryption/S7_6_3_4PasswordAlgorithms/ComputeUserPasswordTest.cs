using System;
using System.Threading.Tasks;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Encryption;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_6Encryption.S7_6_3_4PasswordAlgorithms
{
    public static class PasswordAttemptFactory
    {
        public static IPasswordSource Create(params string[] passwords)
        {
            int currentPassword = 0;
            var ret = new Mock<IPasswordSource>();
            ret.Setup(i => i.GetPassword()).Returns(() => new ValueTask<(string?, PasswordType)>(
                (NextPassword(), PasswordType.User)));
            return ret.Object;

            string? NextPassword() => currentPassword >= passwords.Length ? null : passwords[currentPassword++];
        }
    }
    public class ComputeUserPasswordTest
    {
        private readonly IPasswordSource password = PasswordAttemptFactory.Create("WrongPassword");
        
        [Theory]
        [InlineData(true,@"<</Encrypt <</Filter/Standard/V 2/R 3/Length 128/P -3904/O 
               <E600ECC20288AD8B0D64A929C6A83EE2517679AA0218BECEEA8B7986726A8CDB>
            /U <38ACA54678D67C003A8193381B0FA1CC101112131415161718191A1B1C1D1E1F>>> 
              /ID [<1521FBE61419FCAD51878CC5D478D5FF> <1521FBE61419FCAD51878CC5D478D5FF> ] >>")] 
        [InlineData(false,@"<</Encrypt <</Filter/Standard/V 2/R 3/Length 128/P -3904/O 
               <E600ECC20288AD8B0D64A929C6A83EE2517679AA0218BECEEA8B7986726A8CDB>
            /U <38ACA54678D67C003A8193381B0FA1CC101112131415161718191A1B1C1D1E1F>>> 
              /ID [<1521FBE61419FCAD51878CC5D478D5FF> <1521FBE61419FCAD51878CC5D478D5FF> ] >>")] 
        public async Task VerifyUserPasswordStream(bool succeed,string trailer)
        {
            var tDict = (PdfDictionary)await trailer.ParseObjectAsync();
            var handler = await  SecurityHandlerFactory.CreateSecurityHandler(
                tDict, await tDict.GetAsync<PdfDictionary>(KnownNames.Encrypt));
            try
            {
                await handler.TryInteactiveLogin(password);
                Assert.True(succeed);
            }
            catch (Exception)
            {
                Assert.False(succeed);
            }
        }
    }
}