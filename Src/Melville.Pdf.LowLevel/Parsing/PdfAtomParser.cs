using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.PdfStreamHolders;

namespace Melville.Pdf.LowLevel.Parsing
{
    public abstract class PdfAtomParser : IPdfObjectParser
    {
        public abstract bool TryParse(
            ref SequenceReader<byte> reader, [NotNullWhen(true)] out PdfObject? obj);

        public async Task<PdfObject> ParseAsync(ParsingSource source)
        {
            while(true)
            {
                var seq = (await source.ReadAsync()).Buffer;
                var finalPosition = Parse(ref seq, out var parsedObject);
                if (parsedObject != null)
                {
                    source.AdvanceTo(finalPosition);
                    return parsedObject;
                }
                source.AdvanceTo(seq.GetPosition(0), seq.End);
            }
        }

        private SequencePosition Parse(ref ReadOnlySequence<byte> seq, out PdfObject? parsedObject)
        {
            var reader = new SequenceReader<byte>(seq);
            if (!TryParse(ref reader, out parsedObject))
            {
                parsedObject = null;
            }
            return reader.Position;
        }
    }
}