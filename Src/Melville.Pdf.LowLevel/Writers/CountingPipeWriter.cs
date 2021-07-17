using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.INPC;

namespace Melville.Pdf.LowLevel.Writers
{
    public partial class CountingPipeWriter: PipeWriter
    {
        public long BytesWritten { get; private set; } = 0;
        [DelegateTo] private readonly PipeWriter innerWriter;

        public CountingPipeWriter(PipeWriter innerWriter)
        {
            this.innerWriter = innerWriter;
        }

        public override void Advance(int bytes)
        {
            BytesWritten += bytes;
            innerWriter.Advance(bytes);
        }

    }
}