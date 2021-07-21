using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Melville.FileSystem;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevelReader.DocumentParts
{
    public partial class StreamDocumentPart : DocumentPart
    {
        private readonly PdfStream source;
        [AutoNotify] private byte[] bytes = Array.Empty<byte>();

        public byte[] BytesGetFilter(byte[] item)
        {
            LoadBytesAsync();
            return item;
        }
        public StreamDocumentPart(string title, IReadOnlyList<DocumentPart> children, PdfStream source) : base(title, children)
        {
            this.source = source;
        }

        public async ValueTask LoadBytesAsync()
        {
            if (bytes.Length > 0) return;
            var streamData = await source.GetRawStream();
            var data = new byte[streamData.Length];
            await streamData.FillBufferAsync(data, 0, data.Length);
            Bytes = data;
        }
    }
}