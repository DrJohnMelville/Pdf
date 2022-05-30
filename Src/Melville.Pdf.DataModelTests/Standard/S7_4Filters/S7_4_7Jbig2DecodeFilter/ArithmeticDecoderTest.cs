using System;
using System.Buffers;
using Melville.Pdf.DataModelTests.ParsingTestUtils;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;
using Xunit;
using Xunit.Abstractions;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class ArithmeticDecoderTest
{
    private readonly ITestOutputHelper testOutput;
    private const string encoded = 
        "84 C7 3B FC E1 A1 43 04 02 20 00 00 41 0D BB 86F4 31 7F FF 88 FF 37 47 1A DB 6A DF FF AC";

    private const string decoded =
        "00 02 00 51 00 00 00 C0 03 52 87 2A AA AA AA AA82 C0 20 00 FC D7 9E F6 BF 7F ED 90 4F 46 A3 BF";

    public ArithmeticDecoderTest(ITestOutputHelper testOutput)
    {
        this.testOutput = testOutput;
    }

    [Fact]
    public void SequenceFromT88AnnexH2()
    {
        var encodedSource = CrerateSequenceReader(encoded);
        var decoder = new MQDecoder(ref encodedSource, 1);
        
        var ansSource = CrerateSequenceReader(decoded);
        var ansReader = new BitReader();

        for (int i = 0; i < 32*8; i++)
        {
            ansReader.TryRead(1, ref ansSource, out var expected);
            testOutput.WriteLine($"Iter: {i} {decoder.DebugState}");
            var actual = decoder.GetBit(ref encodedSource, 0);
            Assert.Equal(expected, actual);
            
        }
    }
 
    private static SequenceReader<byte> CrerateSequenceReader(string source) => 
        new SequenceReader<byte>(new ReadOnlySequence<byte>(source.BitsFromHex()));
}