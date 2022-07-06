using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public class PdfArrayParser: IPdfObjectParser
{
    public async Task<PdfObject> ParseAsync(IParsingReader source)
    {
        var reader = await source.Reader.Source.ReadAsync().CA();
        //This has to succeed because the prior parser looked at the prefix to get here.
        source.Reader.Source.AdvanceTo(reader.Buffer.GetPosition(1));
        #warning consider renting these lists
        var items = new List<PdfObject>();
        while (true)
        {
            var item = await source.RootObjectParser.ParseAsync(source).CA();
            if (item == PdfTokenValues.ArrayTerminator) return new PdfArray(items.ToArray());
            items.Add(item);
        }
    }
}