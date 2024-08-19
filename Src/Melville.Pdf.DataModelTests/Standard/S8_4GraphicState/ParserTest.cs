using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Parsing.ContentStreams;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.Standard.S8_4GraphicState;

public abstract class ParserTest
{
    protected Mock<IContentStreamOperations> Target { get; } = new();

    protected async ValueTask ParseStringAsync(
        string s, IContentStreamOperations? target = null)
    {
        var sut = new ContentStreamParser(target ?? Target.Object);
        using var reader = PipeReaderFromString(s);
        await sut.ParseAsync(reader);
    }

    private static IByteSource PipeReaderFromString(string s)
    {
        using var multiplexSource = MultiplexSourceFactory.Create(s.AsExtendedAsciiBytes());
        return multiplexSource.ReadPipeFrom(0);
    }

    protected async Task TestInputAsync(
        string input, params Expression<Action<IContentStreamOperations>>[] actions)
    {
        await ParseStringAsync(input);
        foreach (var action in actions)
        {
            Target.Verify(action);
        }
        Target.VerifyNoOtherCalls();
    }

    protected async Task TestInputAsync(
        string input, MockBase mock)
    {
        await ParseStringAsync(input, (IContentStreamOperations)mock);
        mock.Verify();
    }

    public class MockBase
    {
        private bool called = false;
        protected void SetCalled() => called = true;
        public void Verify() => Assert.True(called);
    }

}