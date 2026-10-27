using Melville.Parsing.CountingReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Mixins;
using System.Diagnostics;

namespace Melville.Parsing.MultiplexSources;

internal abstract partial class CountedMultiplexSource : IMultiplexSource, ICountedSource
{
    private CountedSourceState state;
    private volatile int pendingReaders;
    private int serialNumber = ICountedSource.NextNonce();

#if DEBUG
    private static readonly LightDebugCountingStrategy ProductionCounter = new();
    private ProductionCountingStrategy counter = ProductionCounter;
    //private ProductionCountingStrategy counter = new HeavyDebugCountingStrategy();

#else
    private static readonly ProductionCountingStrategy ProductionCounter = new();
    private ProductionCountingStrategy counter = ProductionCounter;
#endif
    protected CountedMultiplexSource()
    {
        ;
    }

    protected void ResetState()
    {
        serialNumber = ICountedSource.NextNonce();
        state = CountedSourceState.Open;
    }

#warning -- I need to fix a few missed dispose bugs.
    public void Dispose()
    {
        if (state == CountedSourceState.Closed) return;
        state = CountedSourceState.WaitingForPendingReaders;
        TryCleanUp();
    }


    private void TryCleanUp()
    {
        if (ShouldStayOpen()) return;
        CleanUp();
        state = CountedSourceState.Closed;
        GC.SuppressFinalize(this);
    }

    ~CountedMultiplexSource() => counter.FinalizeOccurred(this);

    private bool ShouldStayOpen()
    {
        return state == CountedSourceState.Open || pendingReaders > 0;
    }

    public Stream ReadFrom(long position) => 
        counter.IssueTicket(this, ReadFromOverride(position));

    public IAsyncReader ReaderFrom(long position) =>
        counter.IssueTicket(this, ReaderFromOverride(position));

    public void TryRelease(IHasSourceTicket item) => counter.CancelTicket(this, item);

 
    public IMultiplexSource OffsetFrom(uint offset) => new OffsetMultiplexSource(this, offset);

    public abstract long Length { get; }

    protected abstract Stream ReadFromOverride(long position);
    protected abstract IAsyncReader ReaderFromOverride(long position);

    protected abstract void CleanUp();

    internal enum CountedSourceState
    {
        Open = 0,
        WaitingForPendingReaders = 1,
        Closed = 2
    }
}

