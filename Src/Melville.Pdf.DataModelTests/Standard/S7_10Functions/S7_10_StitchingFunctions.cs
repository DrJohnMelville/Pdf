using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.Builder;
using Melville.Pdf.ReferenceDocumentGenerator.DocumentTypes.LowLevel;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_10Functions
{
    public class S7_10_StitchingFunctions
    {
        private PdfDictionary LinearMapping(int min, int max)
        {
            var builder = new ExponentialFunctionBuilder(1);
            builder.AddFunction(min, max);
            return builder.Create();
        }

        [Fact]
        public async Task SimpleStitch()
        {
            var builder = new StitchingFunctionBuilder(0);

            builder.AddFunction(LinearMapping(0, 1), 0.5);
            builder.AddFunction(LinearMapping(2,3), 1.0, (2,3));
            var stitched = builder.Create();
            await stitched.VerifyNumber(KnownNames.FunctionType, 3);
            await stitched.VerifyPdfDoubleArray(KnownNames.Domain, 0, 1.0);
            await stitched.VerifyPdfDoubleArray(KnownNames.Bounds, 0.5);
            await stitched.VerifyPdfDoubleArray(KnownNames.Encode, 0, 0.5, 2, 3);
        }

        [Fact]
        public void CannotAddBelowMinimum()
        {
            var builder = new StitchingFunctionBuilder(0);
            Assert.Throws<ArgumentException>(() => builder.AddFunction(new PdfDictionary(), -1));
        }
        [Fact]
        public void CannotAddBelowLast()
        {
            var builder = new StitchingFunctionBuilder(0);
            builder.AddFunction(new PdfDictionary(), 0.5);
            Assert.Throws<ArgumentException>(() => builder.AddFunction(new PdfDictionary(), 0.25));
        }
    }
}