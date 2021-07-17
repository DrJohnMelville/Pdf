using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.LowLevel.Writers.DocumentWriters
{
    public static class TrailerWriter
    {
        private static readonly byte[] trailerTag =
            {116, 114, 97, 105, 108, 101, 114, 13, 10}; // trailer\r\n
        private static readonly byte[] startXrefTag = 
            {13, 10, 115, 116, 97, 114, 116, 120, 114, 101, 102, 13, 10}; //\r\nstartxref\r\n
        private static readonly byte[] eofTag = {13, 10, 37, 37, 69, 79, 70}; //\r\n%%EOF 
        public static async Task WriteTrailer(
            PipeWriter target, PdfDictionary dictionary, long xRefStart)
        {
            target.WriteBytes(trailerTag);
            await dictionary.Visit(new PdfObjectWriter(target));
            target.WriteBytes(startXrefTag);
            IntegerWriter.Write(target, xRefStart);
            target.WriteBytes(eofTag);
            await target.FlushAsync();
        }
    }
}