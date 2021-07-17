using System.IO.Pipelines;
using System.Linq;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.LowLevel;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public class LowLevelDocumentWriter
    {
        private readonly CountingPipeWriter target;

        public LowLevelDocumentWriter(PipeWriter target)
        {
            this.target = new CountingPipeWriter(target);
        }

        public async Task WriteAsync(PdfLowLevelDocument document)
        {
            HeaderWriter.WriteHeader(target, document.MajorVersion, document.MinorVersion);
            await ((PipeWriter)target).FlushAsync();

            var maxObject = document.Objects.Keys.Max(i => i.ObjectNumber);
            var positions = new long[maxObject + 1];
            foreach (var item in document.Objects.Values)
            {
                positions[item.Target.ObjectNumber] = target.BytesWritten;
                
            }
        }
    }
}