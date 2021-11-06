using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S14_6MarkedContent;

public class DictionarySkipperTest
{
    [Theory]
    [InlineData("<</Type/Catalog>>")]
    [InlineData("<</Type <11223344>>>")]
    [InlineData("<</Type (>>)>>")]
    [InlineData("<</Type ((hello)>>)>>")]
    [InlineData("<</Type ((\\()>>)>>")]
    [InlineData("<</Type ((\\))>>)>>")]
    [InlineData("<</Type<</Type/Catalog>>>>")]
    public void TestSkipper(string s)
    {
        var span = (s+"  ").AsExtendedAsciiBytes().AsMemory();
        Assert.Equal(s.Length, ExtractedLength(span));
        for (int i = 2; i < span.Length-1; i++)
        {
            Assert.Equal(-1, ExtractedLength(span[..i]));
            
        }
    }

    private long ExtractedLength(in Memory<byte> span)
    {
        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(span));
        var skipper = new DictionarySkipper(ref reader);
        if (!skipper.TrySkipDictionary()) return -1;
        return reader.UnreadSequence.Slice(reader.Position, 
            skipper.CurrentPosition).Length;
    }
}