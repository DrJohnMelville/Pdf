using System.Buffers;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Melville.Pdf.LowLevel.Model;

namespace Melville.Pdf.LowLevel.Parsing.NameParsing
{
    public class PdfArrayParser: PdfAtomParser, IWantRoot
    {
        private IPdfObjectParser rootParser;

        public PdfArrayParser()
        {
            this.rootParser = this;
        }

        public override bool TryParse(ref SequenceReader<byte> reader, out PdfObject? obj)
        {
            obj = null;
            var items = new List<PdfObject>();
            if (!reader.TryAdvance(1)) return false;
            if (!NextTokenFinder.SkipToNextToken(ref reader)) return false;
            while (true)
            {
                if (!reader.TryPeek(out var peek)) return false;
                if (peek == (byte) ']')
                {
                    reader.Advance(1);
                    obj = new PdfArray(items.ToArray());
                    return NextTokenFinder.SkipToNextToken(ref reader);
                }

                if (!((PdfAtomParser)rootParser).TryParse(ref reader, out var temp)) return false;
                items.Add(temp);
            }
        }

        public void SetRoot(IPdfObjectParser rootParser) => this.rootParser = rootParser;
    }
}