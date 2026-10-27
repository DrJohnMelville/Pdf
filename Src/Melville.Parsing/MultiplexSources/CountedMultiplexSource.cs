using Melville.Parsing.CountingReaders;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.Mixins;
using System.Diagnostics;

namespace Melville.Parsing.MultiplexSources;

internal abstract class CountedMultiplexSource : IMultiplexSource, ICountedSource
{
    private CountedSourceState state;
    private int pendingReaders;
    private int serialNumber = ICountedSource.NextNonce();

    protected CountedMultiplexSource()
    {
        RequireDisposal.ReportCreation(this);
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

    ~CountedMultiplexSource() => RequireDisposal.ReportMissedDispose(this);

    private bool ShouldStayOpen()
    {
        return state == CountedSourceState.Open || pendingReaders > 0;
    }

    public Stream ReadFrom(long position) => 
        ReadFromOverride(position, CreateSourceTicket());

    public IAsyncReader ReaderFrom(long position) =>
        ReaderFromOverride(position, CreateSourceTicket());



    public CountedSourceTicket CreateSourceTicket()
    {
        Debug.Assert(state == CountedSourceState.Open);
        pendingReaders++;
        return new CountedSourceTicket(this, serialNumber);
    }

    public bool TryRelease(ref CountedSourceTicket ticket)
    {
        if (!ticket.HasNonce(serialNumber))
        {
            Debug.Fail("Nonce does not match");
            return false;
        }
        ticket = default; // clear the ticket so it cannot be fired again
        ReleaseReference();
        return true;
    }

    private void ReleaseReference()
    {
        Debug.Assert(state != CountedSourceState.Closed);
        Debug.Assert(pendingReaders > 0);
        pendingReaders --;
        TryCleanUp();
    }

    public IMultiplexSource OffsetFrom(uint offset) => new OffsetMultiplexSource(this, offset);

    public abstract long Length { get; }

    protected abstract Stream ReadFromOverride(long position, CountedSourceTicket ticket);
    protected abstract IAsyncReader ReaderFromOverride(long position, CountedSourceTicket ticket);

    protected abstract void CleanUp();

    internal enum CountedSourceState
    {
        Open = 0,
        WaitingForPendingReaders = 1,
        Closed = 2
    }
}

