using System;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Melville.INPC;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams.Bases;
using Melville.StreamInterfaces;
using Xunit;

namespace Melville.Pdf.DataModelTests.MultiplexSource;

internal partial class ConcreteCountedMultiplexSource : CountedMultiplexSource
{
    public int CleanUps {get; private set; }

    public override long Length => throw new System.NotSupportedException();

    protected override Stream ReadFromOverride(long position, CountedSourceTicket ticket) =>
        new Wrapper(false,false, false, ticket);

    protected override IAsyncReader ReaderFromOverride(long position, CountedSourceTicket ticket) =>
        new ReaderWrapper(ticket);

    protected override void CleanUp() => CleanUps++;

    private partial class ReaderWrapper(CountedSourceTicket ticket) : IAsyncReader, IDisposable
    {
        public ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellation = default) =>
            ValueTask.FromResult(0);

        public void Dispose() => ticket.TryRelease();
    }

    private partial class Wrapper : DefaultBaseStream
    {
        [FromConstructor] private CountedSourceTicket ticket;

        protected override void Dispose(bool disposing)
        {
            ticket.TryRelease();
            base.Dispose(disposing);
        }
    }
}

public class CountedMultiplexSourceTest
{
    private readonly ConcreteCountedMultiplexSource sut = new();
    
    [Fact]
    public void CleanupOnDisposeWhenUnused()
    {
        sut.CleanUps.Should().Be(0);
        sut.Dispose();
        sut.CleanUps.Should().Be(1);
    }

    [Fact]
    public void DisposeIsIdempotend()
    {
        sut.CleanUps.Should().Be(0);
        sut.Dispose();
        sut.Dispose();
        sut.CleanUps.Should().Be(1);
    }

    [Fact]
    public void ReadingStreamKeepsSourceOpen()
    {
        var stream = sut.ReadFrom(0);
        sut.Dispose();
        sut.CleanUps.Should().Be(0);
        stream.Dispose();
        sut.CleanUps.Should().Be(1);
    }
}