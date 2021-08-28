using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Filters.Predictors;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters
{
    [MacroItem("ABCDFabcdf", "014101010102016101010102", "Sub")]
    [MacroCode("public class ~2~:StreamTestBase { public ~2~():base(\"~0~\",\"~1~\", KnownNames.ASCIIHexDecode, new PdfDictionary( new Dictionary<PdfName, PdfObject>() { { KnownNames.Predictor, new PdfInteger(11) }, { KnownNames.Colors, new PdfInteger(2) }, { KnownNames.BitsPerComponent, new PdfInteger(4) }, { KnownNames.Columns, new PdfInteger(5) }})){}}")]
    public partial class S7_4_4_4PredictorFunctions
    {
        [Theory]
        [InlineData(0, 1, 2)]
        [InlineData(120, 21,233)]
        public void NonePredictorTest(byte upperLeft, byte up, byte left)
        {
            Assert.Equal(0, new NonePngPredictor().Predict(upperLeft, up, left));
        }
        [Theory]
        [InlineData(0, 1, 2, 2)]
        [InlineData(10, 134, 52, 52)]
        public void SubPredictorTest(byte upperLeft, byte up, byte left, byte result)
        {
            Assert.Equal(result, new SubPngPredictor().Predict(upperLeft, up, left));
        }

        [Theory]
        [InlineData(0, 1, 2, 1)]
        [InlineData(10, 134, 52, 134)]
        public void UpPredictorTest(byte upperLeft, byte up, byte left, byte result)
        {
            Assert.Equal(result, new UpPngPredictor().Predict(upperLeft, up, left));
        }
        [Theory]
        [InlineData(0, 1, 2, 1)]
        [InlineData(10, 134, 52, 93)]
        public void AveragePredictorTest(byte upperLeft, byte up, byte left, byte result)
        {
            Assert.Equal(result, new AveragePngPredictor().Predict(upperLeft, up, left));
        }

        [Theory]
        [InlineData(1, 84, 37, 84)]
        [InlineData(125, 128, 118, 118)]
        [InlineData(61,84,37, 61)]
        public void PaethPredictorTest(byte upperLeft, byte up, byte left, byte result)
        {
            Assert.Equal(result, new PaethPngPredictor().Predict(upperLeft, up, left));
        }

        public static TheoryData<IPngPredictor> Predictors => new TheoryData<IPngPredictor>()
        {
            new NonePngPredictor(),
            new SubPngPredictor(),
            new UpPngPredictor(),
            new AveragePngPredictor(),
            new PaethPngPredictor()
        };

        [Theory]
        [MemberData(nameof(Predictors))]
        public void AllPossibleRoundTripTests(IPngPredictor pred)
        {
            for (int i = 0; i < 256; i++)
            {
                for (int l = 0; l < 256; l++)
                {
                    var val = pred.Encode((byte)i, (byte)(i+3), (byte)(i+10), (byte)l);
                    Assert.Equal(l, pred.Decode((byte)i,(byte)(i+3),(byte)(i+10), val));
                }
            }
        }
        
    }
}