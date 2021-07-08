using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.PdfStreamHolders;

namespace Melville.Pdf.LowLevel.Parsing.NameParsing
{
    public class PdfArrayParser: IPdfObjectParser
    {
        public async Task<PdfObject> ParseAsync(ParsingSource source)
        {
            var reader = await source.ReadAsync();
            source.AdvanceTo(reader.Buffer.GetPosition(1));
            //TODO: consider renting these lists
            var items = new List<PdfObject>();
            while (true)
            {
                var item = await source.RootParser.ParseAsync(source);
                if (item == PdfNull.ArrayTerminator) return new PdfArray(items.ToArray());
                items.Add(item);
            }
        }
    }
}