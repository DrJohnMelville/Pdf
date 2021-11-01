using System.IO;
using System.Net;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.FuzzTest;

public static class ParseFile
{
    public static async ValueTask Do(string fileName)
    {
        await using var stream = File.OpenRead(fileName);
        await Do(stream);
    }

    private static async ValueTask Do(FileStream source)
    {
        var visitor = new TouchEverythingVisitor();
        var lld = await RandomAccessFileParser.Parse(source);
        foreach (var item in lld.Objects.Values)
        {
            item.Visit(visitor);
        }
    }
        
    private class TouchEverythingVisitor: RecursiveDescentVisitor<int>
    {
            
    }
}