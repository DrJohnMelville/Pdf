using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
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
    internal static async Task<PdfDictionary> Read(ParsingFileOwner owner, long offset)
    {
        PdfObject? xRefStreamAsPdfObject ;
        var context = await owner.RentReader(offset).CA();
        xRefStreamAsPdfObject = await context.RootObjectParser.ParseAsync(context).CA();

        if (!(xRefStreamAsPdfObject is PdfStream crossRefPdfStream))
            throw new PdfParseException("Object pointed to by StartXref is not a stream");
        await ReadXrefStreamData(owner, crossRefPdfStream).CA();
        await owner.InitializeDecryption(crossRefPdfStream).CA();

        return crossRefPdfStream;
    }

    /// <summary>
    /// Parse a XrefStream into a sequence of calls to the IIndirectObjectRegistry object.
    /// </summary>
    /// <param name="owner">The target of the parsing operation.</param>
    /// <param name="crossRefPdfStream">The XRefStream to parse.</param>
    /// <returns>A task object representing completion of the operation.</returns>
    public static async Task ReadXrefStreamData(IIndirectObjectRegistry owner, PdfStream crossRefPdfStream)
    {
        var parser = await new XrefStreamParserFactory(crossRefPdfStream, owner).Create().CA();
        await parser.Parse(PipeReader.Create(await crossRefPdfStream.StreamContentAsync().CA())).CA();

    }
}