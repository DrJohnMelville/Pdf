using Melville.INPC;
using System.Diagnostics;
using System.Net.Sockets;

namespace Melville.Parsing.MultiplexSources;

internal abstract partial class CountedMultiplexSource
{
    private static readonly ProductionCountingStrategy ProductionCounter = new();

    private class ProductionCountingStrategy 
    {
        public virtual void CancelTicket(CountedMultiplexSource parent, IHasSourceTicket item)
        {
            if (!item.SourceTicket.HasNonce(parent.serialNumber))
            {
                Debug.Fail("Nonce does not match");
                return;
            }
            Debug.Assert(parent.state != CountedSourceState.Closed);
            Debug.Assert(parent.pendingReaders > 0);

            Interlocked.Decrement(ref parent.pendingReaders);
            item.SourceTicket = default;

            parent.TryCleanUp();
        }

        public virtual T IssueTicket<T>(CountedMultiplexSource parent, T rawItem)
        {
            var item = ((IHasSourceTicket)rawItem);
            Debug.Assert(parent.state is CountedSourceState.Open);
            Interlocked.Increment(ref parent.pendingReaders);
            item.SourceTicket = new CountedSourceTicket(parent, parent.serialNumber);
            return rawItem;
        }

        internal virtual void FinalizeOccurred(CountedMultiplexSource parent)
        {
            parent.CleanUp();
        }
    }
}


