using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Encryption;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Performance.Playground.Encryption
{
    [MemoryDiagnoser()]
    public class KeyChecking
    {
        private const string V2R3128RC4CipherWithBlankUserPasswordFromExampleFile = @"<</Encrypt <</Filter/Standard/V 2/R 3/Length 128/P -3904/O 
               <E600ECC20288AD8B0D64A929C6A83EE2517679AA0218BECEEA8B7986726A8CDB>
            /U <38ACA54678D67C003A8193381B0FA1CC101112131415161718191A1B1C1D1E1F>>> 
              /ID [<1521FBE61419FCAD51878CC5D478D5FF> <1521FBE61419FCAD51878CC5D478D5FF> ] >>";

        private ISecurityHandler handler;

        public KeyChecking()
        {
           handler = Initialize().GetAwaiter().GetResult();
        }

        private async ValueTask<ISecurityHandler> Initialize()
        {
            var tDict = (PdfDictionary)await V2R3128RC4CipherWithBlankUserPasswordFromExampleFile.ParseObjectAsync();
            return await  SecurityHandlerFactory.CreateSecurityHandler(
                tDict, await tDict.GetAsync<PdfDictionary>(KnownNames.Encrypt));

        }

        [Benchmark]
        public void ComputeUserPassword()
        {
            handler.TryComputeRootKey("User", PasswordType.User);
        }
        [Benchmark]
        public void ComputeOwnerPassword()
        {
            handler.TryComputeRootKey("Owner", PasswordType.Owner);
        }
    }
}