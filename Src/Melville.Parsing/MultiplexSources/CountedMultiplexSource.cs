﻿using System.Diagnostics;
using Melville.Parsing.CountingReaders;

namespace Melville.Parsing.MultiplexSources;

internal abstract class CountedMultiplexSource : IMultiplexSource, ICountedSource
{
    private CountedSourceState state;
    private int pendingReaders;
    private int serialNumber = ICountedSource.NextNonce();

    protected void ResetState()
    {
        serialNumber = ICountedSource.NextNonce();
        state = CountedSourceState.Open;
    }

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
    }

    private bool ShouldStayOpen()
    {
        return state == CountedSourceState.Open || pendingReaders > 0;
    }

    public Stream ReadFrom(long position) => 
        ReadFromOverride(position, CreateSourceTicket());

    private CountedSourceTicket CreateSourceTicket()
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

    public IByteSource ReadPipeFrom(long position, long startingPosition = 0) => 
        ReadFromPipeOverride(position, startingPosition, CreateSourceTicket());

    public IMultiplexSource  OffsetFrom(uint offset)
    {
        return new OffsetMultiplexSource(this, offset);
    }

    public abstract long Length { get; }

    protected abstract Stream ReadFromOverride(long position, CountedSourceTicket ticket);

    protected virtual IByteSource ReadFromPipeOverride(long position, long startingPosition, CountedSourceTicket ticket) =>
        MultiplexSourceFactory.SingleReaderForStream(this.ReadFromOverride(position, ticket))
            .WithCurrentPosition(startingPosition);

    protected abstract void CleanUp();

    internal enum CountedSourceState
    {
        Open = 0,
        WaitingForPendingReaders = 1,
        Closed = 2
    }
}

