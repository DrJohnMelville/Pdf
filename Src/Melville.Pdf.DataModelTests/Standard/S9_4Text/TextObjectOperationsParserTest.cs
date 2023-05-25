using System;
using System.Buffers;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Wrappers.ContentValueStreamUnions;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S9_4Text;

public partial class TextObjectOperationsParserTest : ParserTest
{
    [Fact]
    public Task BeginTextObjectAsync() => TestInputAsync("BT", i => i.BeginTextObject());
    [Fact]
    public Task EndTextObjectAsync() => TestInputAsync("ET", i => i.EndTextObject());
    [Fact]
    public Task MovePositionByAsync() => TestInputAsync("5 6 Td", i => i.MovePositionBy(5,6));
    [Fact]
    public Task MovePositionByWithLeadingAsync() => 
        TestInputAsync("5 6 TD", i => i.MovePositionByWithLeading(5,6));
    [Fact]
    public Task SetMatrixAsync() => 
        TestInputAsync("6 5 4 3 2 1 Tm", i => i.SetTextMatrix(6,5,4,3,2,1));
    [Fact]
    public Task MoveToNextLineAsync() => 
        TestInputAsync("T*", i => i.MoveToNextTextLine());

    private partial class TjMock : MockBase, IContentStreamOperations
    {
        [DelegateTo()] private IContentStreamOperations op = null!;

        public ValueTask ShowStringAsync(ReadOnlyMemory<byte> input)
        {
            AssertResult(input, "ABC");
            return new ValueTask();
        }

        public ValueTask MoveToNextLineAndShowStringAsync(ReadOnlyMemory<byte> input)
        {
            AssertResult(input, "def");
            return new ValueTask();
        }

        public ValueTask MoveToNextLineAndShowStringAsync(
            double wordSpace, double charSpace,ReadOnlyMemory<byte> input)
        {
            Assert.Equal(7, wordSpace);
            Assert.Equal(8, charSpace);
            AssertResult(input, "IJK");
            return new ValueTask();
        }

        public ValueTask ShowSpacedStringAsync(in Span<ContentStreamValueUnion> values)
        {
            //[(a)2(b)3(c)(s)4 5]TJ
            Assert.Equal("a", ExtendedAsciiEncoding.ExtendedAsciiString(values[0].Bytes.Span));
            Assert.Equal(2, values[1].Integer);
            Assert.Equal("b", ExtendedAsciiEncoding.ExtendedAsciiString(values[2].Bytes.Span));
            Assert.Equal(3, values[3].Integer);
            Assert.Equal("c", ExtendedAsciiEncoding.ExtendedAsciiString(values[4].Bytes.Span));
            Assert.Equal("s", ExtendedAsciiEncoding.ExtendedAsciiString(values[5].Bytes.Span));
            Assert.Equal(4, values[6].Integer);
            Assert.Equal(5, values[7].Integer);

            SetCalled();
            return ValueTask.CompletedTask;
        }

        private void AssertResult(ReadOnlyMemory<byte> input, string expected)
        {
            Assert.Equal(expected, input.Span.ExtendedAsciiString());
            SetCalled();
        }
    }
    [Fact]
    public Task ParseShowSyntaxStringAsync() => TestInputAsync("(ABC) Tj", new TjMock());
    [Fact]
    public Task ParseShowHexStringAsync() => TestInputAsync("<4142 43> Tj", new TjMock());
    [Fact]
    public Task MoveToNextLineAndShowAsync() => TestInputAsync("(def)'", new TjMock());
    [Fact]
    public Task MoveToNextLineAndShow2Async() => TestInputAsync("7 8(IJK)\"", new TjMock());
    [Fact]
    public Task ShowSpacedStringAsync() => TestInputAsync("[(a)2(b)3(c)(s)4 5]TJ", new TjMock());
    
}