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
    public Task BeginTextObject() => TestInput("BT", i => i.BeginTextObject());
    [Fact]
    public Task EndTextObject() => TestInput("ET", i => i.EndTextObject());
    [Fact]
    public Task MovePositionBy() => TestInput("5 6 Td", i => i.MovePositionBy(5,6));
    [Fact]
    public Task MovePositionByWithLeading() => 
        TestInput("5 6 TD", i => i.MovePositionByWithLeading(5,6));
    [Fact]
    public Task SetMatrix() => 
        TestInput("6 5 4 3 2 1 Tm", i => i.SetTextMatrix(6,5,4,3,2,1));
    [Fact]
    public Task MoveToNextLine() => 
        TestInput("T*", i => i.MoveToNextTextLine());

    private partial class TjMock : MockBase, IContentStreamOperationses
    {
        [DelegateTo()] private IContentStreamOperationses op = null!;

        public void ShowString(in ReadOnlyMemory<byte> input) => AssertResult(input, "ABC");
        public void MoveToNextLineAndShowString(in ReadOnlyMemory<byte> input) => AssertResult(input, "def");
        public void MoveToNextLineAndShowString(
            double wordSpace, double charSpace,in ReadOnlyMemory<byte> input)
        {
            Assert.Equal(7, wordSpace);
            Assert.Equal(8, charSpace);
            AssertResult(input, "IJK");
        }

        public void ShowSpacedString(in InterleavedArray<Memory<byte>, double> values)
        {
            var target = new Mock<IInterleavedTarget<Memory<byte>, double>>(MockBehavior.Strict);
            var seq = new MockSequence();
            target.InSequence(seq).Setup(i => i.Handle(It.IsAny<Memory<byte>>()));
            target.InSequence(seq).Setup(i => i.Handle(2.0));
            target.InSequence(seq).Setup(i => i.Handle(It.IsAny<Memory<byte>>()));
            target.InSequence(seq).Setup(i => i.Handle(3.0));
            target.InSequence(seq).Setup(i => i.Handle(It.IsAny<Memory<byte>>()));
            target.InSequence(seq).Setup(i => i.Handle(It.IsAny<Memory<byte>>()));
            target.InSequence(seq).Setup(i => i.Handle(4.0));
            target.InSequence(seq).Setup(i => i.Handle(5.0));
            values.Iterate(target.Object);
            target.Verify(i => i.Handle(It.IsAny<Memory<byte>>()), Times.Exactly(4));
            target.Verify(i => i.Handle(It.IsAny<double>()), Times.Exactly(4));
            target.VerifyNoOtherCalls();
            SetCalled();
        }

        private void AssertResult(ReadOnlyMemory<byte> input, string expected)
        {
            Assert.Equal(expected, input.Span.ExtendedAsciiString());
            SetCalled();
        }
    }
    [Fact]
    public Task ParseShowSyntaxString() => TestInput("(ABC) Tj", new TjMock());
    [Fact]
    public Task ParseShowHexString() => TestInput("<4142 43> Tj", new TjMock());
    [Fact]
    public Task MoveToNextLineAndShow() => TestInput("(def)'", new TjMock());
    [Fact]
    public Task MoveToNextLineAndShow2() => TestInput("7 8(IJK)\"", new TjMock());
    [Fact]
    public Task ShowSpacedString() => TestInput("[(a)2(b)3(c)(s)4 5]TJ", new TjMock());
    
}