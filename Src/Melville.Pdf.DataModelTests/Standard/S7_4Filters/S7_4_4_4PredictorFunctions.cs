using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.Predictors;
using Xunit;
//Rider reports that the usings are unneeded, it does not know the Macro expander uses it.
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Melville.Pdf.DataModelTests.StreamUtilities;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters;

[MacroItem("ABCDFabcdf", "004142434446006162636466", "None", "10")]
[MacroItem("ABCDFabcdf", "014101010102016101010102", "Sub", "11")]
[MacroItem("ABCDFabcdf", "024142434446022020202020", "Up", "12")]
[MacroItem("ABCDFabcdf", "034122222324034111111111", "Average", "13")]
[MacroItem("ABCDFabcdf", "044101010102042001010102", "Paeth", "14")]
[MacroItem("ABCDFabcdf", "014101010102016101010102", "FakeOptimal", "15")]
[MacroCode(
    "public class ~2~:StreamTestBase { public ~2~():base(\"~0~\",\"~1~\", KnownNames.ASCIIHexDecodeTName, new ValueDictionaryBuilder().WithItem(KnownNames.PredictorTName, ~3~).WithItem( KnownNames.ColorsTName, 2).WithItem(KnownNames.BitsPerComponentTName, 4).WithItem(KnownNames.ColumnsTName,5).AsDictionary()){}}")]
public partial class S7_4_4_4PredictorFunctions
{
    [Theory]
    [InlineData(0, 1, 2)]
    [InlineData(120, 21, 233)]
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
    [InlineData(61, 84, 37, 61)]
    public void PaethPredictorTest(byte upperLeft, byte up, byte left, byte result)
    {
        Assert.Equal(result, new PaethPngPredictor().Predict(upperLeft, up, left));
    }

    public static TheoryData<object> Predictors => new()
    {
        new NonePngPredictor(),
        new SubPngPredictor(),
        new UpPngPredictor(),
        new AveragePngPredictor(),
        new PaethPngPredictor()
    };

    [Theory]
    [MemberData(nameof(Predictors))]
    public void AllPossibleRoundTripTests(object pred2)
    {
        var pred = (IPngPredictor)pred2;
        for (int i = 0; i < 256; i++)
        {
            for (int l = 0; l < 256; l++)
            {
                var val = pred.Encode((byte)i, (byte)(i + 3), (byte)(i + 10), (byte)l);
                Assert.Equal(l, pred.Decode((byte)i, (byte)(i + 3), (byte)(i + 10), val));
            }
        }
    }

    [MacroItem("ABCDFabcdf", "0041424344466162636466", "InterlevedNone", "10")]
    [MacroItem("ABCDFabcdf", "0141420202031D1C020203", "InterlevedSub", "11")]
    [MacroItem("ABCDFabcdf", "0241424344466162636466", "InterlevedUp", "12")]
    [MacroItem("ABCDFabcdf", "0341422323253F3F333335", "InterlevedAverage", "13")]
    [MacroItem("ABCDFabcdf", "0441420202031D1C020203", "InterlevedPaeth", "14")]
    [MacroItem("ABCDFabcdf", "0141420202031D1C020203", "InterlevedFakeOptimal", "15")]
    [MacroCode(
        "public class ~2~:StreamTestBase { public ~2~():base(\"~0~\",\"~1~\", KnownNames.ASCIIHexDecodeTName, new ValueDictionaryBuilder().WithItem(KnownNames.PredictorTName, ~3~) .WithItem(KnownNames.ColorsTName, 2).WithItem(KnownNames.BitsPerComponentTName, 8).WithItem( KnownNames.ColumnsTName,5).AsDictionary()){}}")]
    public static int InterlevePngTests = 0;

    // Input, HexEncoded Output, name, colors, bits per color
    [MacroItem("AAAAAAAAAAAAAAAA", "41410000000000000000414100000000", "Two8BitColors", 2, 8)]
    [MacroItem("ABABABABABABABAB", "41420000000000000000414200000000", "AlternatincColors", 2, 8)]
    [MacroItem("\x01\x23\x45\x67\x89\xAB\xCD", "01233333333333", "ThreeFourBitColors", 3, 4)]
    [MacroItem("\x01\x20\x12\x01\x20\x12\x01\x20\x01\x20\x12\x01\x20\x12\x01\x20", "01200000000000000120000000000000",
        "LineEndingPadsBytes", 3, 4)]
    [MacroCode(
        "public class ~2~:StreamTestBase { public ~2~():base(\"~0~\",\"~1~\", KnownNames.ASCIIHexDecodeTName, new ValueDictionaryBuilder().WithItem(KnownNames.PredictorTName, 2).WithItem(KnownNames.ColorsTName, ~3~).WithItem( KnownNames.BitsPerComponentTName, ~4~).WithItem(KnownNames.ColumnsTName, 5).AsDictionary()){}}")]
    private static int TiffPredicter2Tests() => 9;
}