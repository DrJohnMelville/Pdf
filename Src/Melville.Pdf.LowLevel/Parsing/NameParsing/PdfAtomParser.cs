using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model;

namespace Melville.Pdf.LowLevel.Parsing.NameParsing
{
    public abstract class PdfAtomParser : IPdfObjectParser
    {
        public abstract bool TryParse(
            ref SequenceReader<byte> reader, [NotNullWhen(true)] out PdfObject? obj);

        public override async Task<PdfObject> ParseAsync(PipeReader pr)
        {
            while(true)
            {
                var seq = (await pr.ReadAsync()).Buffer;
                var finalPosition = Parse(ref seq, out var parsedObject);
                if (parsedObject != null)
                {
                    pr.AdvanceTo(finalPosition);
                    return parsedObject;
                }
                pr.AdvanceTo(seq.GetPosition(0), finalPosition);
            }
        }

        private SequencePosition Parse(ref ReadOnlySequence<byte> seq, out PdfObject? parsedObject)
        {
            var reader = new SequenceReader<byte>(seq);
            TryParse(ref reader, out parsedObject);
            return reader.Position;
        }
    }
}