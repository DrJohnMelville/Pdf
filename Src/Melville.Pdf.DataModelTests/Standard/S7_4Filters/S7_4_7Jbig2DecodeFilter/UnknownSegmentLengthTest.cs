using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.JBig2.SegmentParsers;
using Melville.Pdf.ReferenceDocuments.Utility;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class UnknownSegmentLengthTest
{
    [Theory]
    [InlineData("01020304 05060708 09101112 13141516 01 0000 05060708", 0)]
    [InlineData("01020304 05060708 09101112 13141516 03 AABCDE 0000 05060708", 0)]
    [InlineData("01020304 05060708 09101112 13141516 31 AABCDE 0000 05060708 0102", 2)]
    [InlineData("01020304 05060708 09101112 13141516 31 AA00BCDE 0000 05060708 0102", 2)]
    [InlineData("01020304 05060708 09101112 13141516 12 AABCDE FFAC 05060708 0102", 2)]
    [InlineData("01020304 05060708 09101112 13141516 12 AABCDE FFFFAC 05060708 0102", 2)]
    
    [InlineData("01020304 15161718 09101112 13141516 12 AABCDE FFFFAC 05060708 0102", 2)]
    public async Task TrivialMmrSegmentAsync(string source, int extraBytes)
    {
        var data = source.BitsFromHex();
        var reader = PipeReader.Create(new ReadOnlySequence<byte>(data));
        var (ret, shouldAdvance) = await new ReadUnknownSegmentLength(reader).ReadSegmentAsync();
        ret.Length.Should().Be(data.Length-extraBytes);

        var arr = ret.ToArray();
        arr[4].Should().Be(arr[^4]);
        arr[5].Should().Be(arr[^3]);
        arr[6].Should().Be(arr[^2]);
        arr[7].Should().Be(arr[^1]);
    }
}