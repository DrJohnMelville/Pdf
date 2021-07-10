using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.NameParsing
{
    public class PdfArrayParser: IPdfObjectParser
    {
        public async Task<PdfObject> ParseAsync(ParsingSource source)
        {
            var reader = await source.ReadAsync();
            //This has to succeed because the prior parser looked at the prefix to get here.
            source.AdvanceTo(reader.Buffer.GetPosition(1));
            //TODO: consider renting these lists
            var items = new List<PdfObject>();
            while (true)
            {
                var item = await source.RootParser.ParseAsync(source);
                if (item == PdfEmptyConstants.ArrayTerminator) return new PdfArray(items.ToArray());
                items.Add(item);
            }
        }
    }
}