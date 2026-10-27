using Melville.INPC;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;

namespace Melville.Parsing.MultiplexSources;

internal abstract partial class CountedMultiplexSource
{
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

        public virtual void FinalizeOccurred(CountedMultiplexSource parent)
        {
            parent.CleanUp();
        }
    }

    private class LightDebugCountingStrategy:ProductionCountingStrategy
    {
        public override void FinalizeOccurred(CountedMultiplexSource parent)
        {
            base.FinalizeOccurred(parent);
            throw new InvalidOperationException($"Failed to dispose of a {parent}");
        }
    }

    private class HeavyDebugCountingStrategy:ProductionCountingStrategy
    {
        private readonly ConcurrentDictionary<object, StackTrace> readers = new();
        private StackTrace creationStack;

        public HeavyDebugCountingStrategy() => creationStack = new StackTrace();

        public override T IssueTicket<T>(CountedMultiplexSource parent, T rawItem)
        {
            if (!readers.TryAdd(rawItem, new StackTrace()))
                throw new InvalidOperationException("Inserted the same reader twice.");
            return base.IssueTicket(parent, rawItem);
        }

        public override void CancelTicket(CountedMultiplexSource parent, IHasSourceTicket item)
        {
            if (!readers.TryRemove(item, out var _))
            {
                throw new InvalidOperationException("Tried to remove a unkown value.  (Possible doublee dispose)");
            }
            base.CancelTicket(parent, item);
        }

        public override void FinalizeOccurred(CountedMultiplexSource parent)
        {
            if (parent.state is CountedSourceState.Open)
                throw new InvalidOperationException($"""
                    Did not directly call dispose on {parent}, created at
                    {creationStack}
                    """);
            if (readers.Count > 0)
                throw new InvalidOperationException($"""
                    {readers.Count} reader(s) prevented destruction of {parent}.
                    Readers created at:
                    {string.Join(Environment.NewLine, readers.Values)}
                    """);
            throw new InvalidOperationException("Finalize should not have been called");
        }
    }
}


