using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Filters.StreamFilters
{
    public class ConcatStream : SequentialReadFilterStream
    {
        private Stream? current = null;
        private IEnumerator<Stream> items;

        public ConcatStream(params Stream[] items) : this(items.AsEnumerable()) { }
        public ConcatStream(IEnumerable<Stream> items)
        {
            this.items = items.GetEnumerator();
            GetNextSource();
        }
        public override async ValueTask<int> ReadAsync(
            Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var bytesWritten = 0;
            while (!AtEndOfStream() && bytesWritten < buffer.Length)
            {
                var localWritten = await current.ReadAsync(buffer[bytesWritten..], cancellationToken);
                bytesWritten += localWritten;
                ComputeNextSource(localWritten);
            }

            return bytesWritten;
        }

        [MemberNotNullWhen(false, nameof(current))]
        private bool AtEndOfStream() => current is null;
        private void ComputeNextSource(int localWritten)
        {
            if (localWritten == 0) GetNextSource();
        }
        private void GetNextSource() => current = items.MoveNext() ? items.Current : null;
    }
}