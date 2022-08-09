using System.Collections.Generic;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public class PdfArrayParser : IPdfObjectParser
{
    public async Task<PdfObject> ParseAsync(IParsingReader source)
    {
        var reader = await source.Reader.Source.ReadAsync().CA();
        //This has to succeed because the prior parser looked at the prefix to get here.
        source.Reader.Source.AdvanceTo(reader.Buffer.GetPosition(1));
        return new PdfArray(await RecursiveReadArray(source, 0).CA());
    }

    private const int RecursiveLimit = 1000;

    private static async ValueTask<PdfObject[]> RecursiveReadArray(
        IParsingReader reader, int currentLocation)
    {
        // reading the array recursively allows us to use the stack, rather than the heap, to store intermediate values
        // until we discover the exact size of array that we need.
        //
        // Very big arrays can blow the stack of course, after 1000 iterations, which is bigger than 99% of pdf arrays
        // we bail out to a heap based solution
        var element = await reader.RootObjectParser.ParseAsync(reader).CA();
        if (element == PdfTokenValues.ArrayTerminator) return new PdfObject[currentLocation];
        if (currentLocation >= RecursiveLimit) return await ReadArrayElements(reader).CA();
        var ret = await RecursiveReadArray(reader, currentLocation + 1).CA();
        ret[currentLocation] = element;
        return ret;
    }

    private static async ValueTask<PdfObject[]> ReadArrayElements(IParsingReader reader)
    {
        var items = new List<PdfObject>();
        while (true)
        {
            var item = await reader.RootObjectParser.ParseAsync(reader).CA();
            if (item == PdfTokenValues.ArrayTerminator) return CopyToHighPositions(items);
            items.Add(item);
        }
    }
    private static PdfObject[] CopyToHighPositions(List<PdfObject> items)
    {
        var ret = new PdfObject[items.Count + RecursiveLimit];
        int position = RecursiveLimit;
        foreach (var item in items)
        {
            ret[position++] = item;
        }
        return ret;
    }
}