using System;
using System.Buffers;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S14_6MarkedContent;

public partial class MarkedContentParser : ParserTest
{
    [Fact]
    public Task MarkedContentPoint() =>
        TestInput("/M1 MP", i => i.MarkedContentPoint(NameDirectory.Get("M1")));
    [Fact]
    public Task MarkedContentPointWithNamedParam() =>
        TestInput("/M1 /M2 DP", i => i.MarkedContentPoint("M1", "M2"));

    private partial class MarkedContentPointMock: MockBase, IContentStreamOperations 
    {
        [DelegateTo()] private readonly IContentStreamOperations fake = null!;
        private readonly string expected;

        public MarkedContentPointMock(string expected)
        {
            this.expected = expected;
        }

        public void MarkedContentPoint(PdfName tag, in UnparsedDictionary dict)
        {
            Assert.Equal(expected, StringFromDict(dict));
            SetCalled();
        }

        private static string StringFromDict(UnparsedDictionary dict) => 
            ExtendedAsciiEncoding.ExtendedAsciiString(dict.Text.Span);
    }
    
    [Theory]
    [InlineData("<</Type/Catalog>>")]
    [InlineData("<</Type<</Type/Catalog>>>>")]
    public Task MarkedContentPointWithInlineDict(string dictStr) => 
        TestInput($"/M1{dictStr}DP", new MarkedContentPointMock(dictStr));

    [Theory]
    [InlineData("<</Type/Catalog>>")]
    [InlineData("<</Type<</Type/Catalog>>>>")]
    public void TestSkipper(string s)
    {
        var reader = new SequenceReader<byte>(new ReadOnlySequence<byte>(
            (s+"  ").AsExtendedAsciiBytes()));
        var skipper = new DictionarySkipper(ref reader);
        Assert.True(skipper.TrySkipDictionary());
        Assert.Equal(s.Length+1, 
            reader.UnreadSequence.Slice(reader.Position, 
                    skipper.CurrentPosition).Length);
        
    }
}