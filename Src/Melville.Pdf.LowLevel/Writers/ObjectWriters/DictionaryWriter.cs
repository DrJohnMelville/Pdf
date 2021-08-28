using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters
{
    public static class DictionaryWriter
    {
        public static async ValueTask<FlushResult> Write(
            PipeWriter writer, ILowLevelVisitor<ValueTask<FlushResult>> innerWriter, 
            IReadOnlyDictionary<PdfName, PdfObject> items)
        {
            writer.WriteBytes((byte)'<',(byte)'<');
            int position = 0;
            #warning -- can eliminate the white space in dictionary for better packing.
            foreach (var item in items)
            {
                if (position++ > 0)
                {
                    writer.WriteSpace();
                }

                await item.Key.Visit(innerWriter);
                writer.WriteSpace();
                await writer.FlushAsync();
                await item.Value.Visit(innerWriter);
            }
            writer.WriteBytes((byte)'>',(byte)'>');
            return await writer.FlushAsync();
        }
    }
}