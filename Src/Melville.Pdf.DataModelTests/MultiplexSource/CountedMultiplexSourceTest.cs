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

    protected override Stream ReadFromOverride(long position) =>
        new Wrapper(false,false, false);

    protected override IAsyncReader ReaderFromOverride(long position) =>
        new ReaderWrapper();

    protected override void CleanUp() => CleanUps++;

    private partial class ReaderWrapper : IAsyncReader, IDisposable, IHasSourceTicket
    {
        CountedSourceTicket IHasSourceTicket.SourceTicket { get; set; }

        public ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellation = default) =>
            ValueTask.FromResult(0);

        public void Dispose() => ((IHasSourceTicket)this).SourceTicket.Release(this);
    }

    [FromConstructor]
    private partial class Wrapper : DefaultBaseStream, IHasSourceTicket
    {
        CountedSourceTicket IHasSourceTicket.SourceTicket { get; set; }

        protected override void Dispose(bool disposing)
        {
            ((IHasSourceTicket)this).SourceTicket.Release(this);
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