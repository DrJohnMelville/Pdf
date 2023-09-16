using System;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
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
        [FromConstructor] private string result;

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

        public ISpacedStringBuilder GetSpacedStringBuilder()
        {
            var ret = new Mock<ISpacedStringBuilder>(MockBehavior.Strict);
            var seq = new MockSequence();
            ret.InSequence(seq).Setup(i => i.SpacedStringComponentAsync(It.IsAny<Memory<byte>>()))
                .Returns((Memory<byte> i) => CheckMemoryAsync(i, "a"u8));
            ret.InSequence(seq).Setup(i => i.SpacedStringComponentAsync(2.0)).Returns(ValueTask.CompletedTask);
            ret.InSequence(seq).Setup(i => i.SpacedStringComponentAsync(It.IsAny<Memory<byte>>()))
                .Returns((Memory<byte> i) => CheckMemoryAsync(i, "b"u8));
            ret.InSequence(seq).Setup(i => i.SpacedStringComponentAsync(3.0)).Returns(ValueTask.CompletedTask);
            ret.InSequence(seq).Setup(i => i.SpacedStringComponentAsync(It.IsAny<Memory<byte>>()))
                .Returns((Memory<byte> i) => CheckMemoryAsync(i, "c"u8));
            ret.InSequence(seq).Setup(i => i.SpacedStringComponentAsync(It.IsAny<Memory<byte>>()))
                .Returns((Memory<byte> i) => CheckMemoryAsync(i, "s"u8));
            ret.InSequence(seq).Setup(i => i.SpacedStringComponentAsync(4.0)).Returns(ValueTask.CompletedTask);
            ret.InSequence(seq).Setup(i => i.SpacedStringComponentAsync(5.0)).Returns(ValueTask.CompletedTask);
            ret.InSequence(seq).Setup(i => i.DoneWritingAsync()).Returns(FinishAsync(ret));
            return ret.Object;
        }

        private ValueTask FinishAsync(Mock<ISpacedStringBuilder> ret)
        {
            ret.VerifyAll();
            SetCalled();
            return ValueTask.CompletedTask;
        }

        private ValueTask CheckMemoryAsync(Memory<byte> actual, ReadOnlySpan<byte> expected)
        {
            Assert.True(expected.SequenceEqual(actual.Span));
            return ValueTask.CompletedTask;
        }


        private void AssertResult(ReadOnlyMemory<byte> input, string expected)
        {
            Assert.Equal(result, input.Span.ExtendedAsciiString());
            SetCalled();
        }


    }
    [Fact]
    public Task ParseShowSyntaxStringAsync() => TestInputAsync("(ABC) Tj", new TjMock("ABC"));
    [Fact]
    public Task ParseOpenCurlyBugAsync() => TestInputAsync("({) Tj", new TjMock("{"));
    [Fact]
    public Task ParseShowHexStringAsync() => TestInputAsync("<4142 43> Tj", new TjMock("ABC"));
    [Fact]
    public Task MoveToNextLineAndShowAsync() => TestInputAsync("(def)'", new TjMock("def"));
    [Fact]
    public Task MoveToNextLineAndShow2Async() => TestInputAsync("7 8(IJK)\"", new TjMock("IJK"));
    [Fact]
    public Task ShowSpacedStringAsync() => TestInputAsync("[(a)2(b)3(c)(s)4 5]TJ", new TjMock("nothing"));
    
}