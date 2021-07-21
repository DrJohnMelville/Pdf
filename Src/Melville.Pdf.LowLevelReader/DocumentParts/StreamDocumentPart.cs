using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Melville.FileSystem;
using Melville.INPC;
using Melville.Linq;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevelReader.DocumentParts
{
    public partial class StreamDocumentPart : DocumentPart
    {
        private readonly PdfStream source;
        [AutoNotify] private string displayContent = "";

        private string DisplayContentGetFilter(string item)
        {
            LoadBytesAsync(item).GetAwaiter();
            return item;
        }
        public StreamDocumentPart(string title, IReadOnlyList<DocumentPart> children, PdfStream source) : base(title, children)
        {
            this.source = source;
        }

        public async ValueTask LoadBytesAsync(string item)
        {
            if (item.Length > 0) return;
            var streamData = await source.GetRawStream();
            var data = new byte[streamData.Length];
            await streamData.FillBufferAsync(data, 0, data.Length);
            DisplayContent = string.Join("\r\n", CreateHexDump(data));
        }

        private static IEnumerable<string> CreateHexDump(byte[] data) => 
            data.BinaryFormat().Select((hexDump, index) => $"{index:X7}0  {hexDump}");
    }
}