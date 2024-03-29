﻿using System.Buffers;
using Melville.JBig2.ArithmeticEncodings;
using Melville.Parsing.VariableBitEncoding;
using Melville.Pdf.ReferenceDocuments.Utility;
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
        var decoder = new MQDecoder();
        
        var ansSource = CrerateSequenceReader(decoded);
        var ansReader = new BitReader();

        var context = new ContextEntry();

        for (int i = 0; i < 32*8; i++)
        {
            ansReader.TryRead(1, ref ansSource, out var expected);
            var actual = decoder.GetBit(ref encodedSource, ref context);
            Assert.Equal(expected, actual);
            
        }
    }
 
    private static SequenceReader<byte> CrerateSequenceReader(string source) => 
        new SequenceReader<byte>(new ReadOnlySequence<byte>(source.BitsFromHex()));
}