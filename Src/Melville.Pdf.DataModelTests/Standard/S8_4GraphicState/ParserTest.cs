using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public abstract class ParserTest
{
    protected Mock<IContentStreamOperationses> Target { get; } = new();

    protected ValueTask ParseString(
        string s, IContentStreamOperationses? target = null)
    {
        var sut = new ContentStreamParser(target ?? Target.Object);
        return sut.Parse(PipeReaderFromString(s));
    }

    private static PipeReader PipeReaderFromString(string s) =>
        PipeReader.Create(new MemoryStream(s.AsExtendedAsciiBytes()));

    protected async Task TestInput(
        string input, Expression<Action<IContentStreamOperationses>> action)
    {
        await ParseString(input);
        Target.Verify(action);
        Target.VerifyNoOtherCalls();
    }

    protected async Task TestInput(
        string input, MockBase mock)
    {
        await ParseString(input, (IContentStreamOperationses)mock);
        mock.Verify();
    }

    public class MockBase
    {
        private bool called = false;
        protected void SetCalled() => called = true;
        public void Verify() => Assert.True(called);
    }

}