using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

/// <summary>
/// This parses a cross reference stream into an IIndirectObjectRegistry.
///
/// This is public so that the LowLevelViewer can show the results of parsing an XRefStream.
/// Most consumers outside of the library will not need to use this class directly.
/// </summary>
public static class CrossReferenceStreamParser
{
    internal static async Task<PdfValueStream> ReadAsync(ParsingFileOwner owner, long offset)
    {
        var context = await owner.RentReaderAsync(offset).CA();

        var xRefStreamAsPdfObject = await context.NewRootObjectParser.ParseTopLevelObject().CA();


        if (!xRefStreamAsPdfObject.TryGet(out PdfValueStream? crossRefPdfStream)) 
            throw new PdfParseException("Object pointed to by StartXref is not a stream");
        await ReadXrefStreamDataAsync(owner, crossRefPdfStream).CA();
        await owner.InitializeDecryptionAsync(crossRefPdfStream).CA();

        return crossRefPdfStream;
    }

    /// <summary>
    /// Parse a XrefStream into a sequence of calls to the IIndirectObjectRegistry object.
    /// </summary>
    /// <param name="owner">The target of the parsing operation.</param>
    /// <param name="crossRefPdfStream">The XRefStream to parse.</param>
    /// <returns>A task object representing completion of the operation.</returns>
    public static async Task ReadXrefStreamDataAsync(IIndirectObjectRegistry owner, PdfValueStream crossRefPdfStream)
    {
        var parser = await new XrefStreamParserFactory(crossRefPdfStream, owner).CreateAsync().CA();
        await parser.ParseAsync(PipeReader.Create(await crossRefPdfStream.StreamContentAsync().CA())).CA();

    }
}