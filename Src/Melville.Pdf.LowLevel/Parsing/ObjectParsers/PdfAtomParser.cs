using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    public abstract class PdfAtomParser : IPdfObjectParser
    {
        public abstract bool TryParse(
            ref SequenceReader<byte> reader, [NotNullWhen(true)] out PdfObject? obj);

        public async Task<PdfObject> ParseAsync(ParsingSource source)
        {
            PdfObject result;
            do{}while(source.ShouldContinue(Parse(await source.ReadAsync(), out result!)));
            return result;
        }

        private (bool Success, SequencePosition Position) Parse(ReadResult source, out PdfObject? result)
        {
            var reader = new SequenceReader<byte>(source.Buffer);
            return (TryParse(ref reader, out result), reader.Position);
        }
    }
}