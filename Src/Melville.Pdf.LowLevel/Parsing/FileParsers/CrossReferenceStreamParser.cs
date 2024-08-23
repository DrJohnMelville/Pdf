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
    internal static async Task<PdfStream> ReadAsync(ParsingFileOwner owner, long offset)
    {
        using var context = owner.SubsetReader(offset);

        var xRefStreamAsPdfObject = await new RootObjectParser(context).ParseTopLevelObjectAsync().CA();


        if (!xRefStreamAsPdfObject.TryGet(out PdfStream? crossRefPdfStream)) 
            throw new PdfParseException("Object pointed to by StartXref is not a stream");
        await ReadXrefStreamDataAsync(owner.NewIndirectResolver, crossRefPdfStream).CA();
        await owner.InitializeDecryptionAsync(crossRefPdfStream).CA();

        return crossRefPdfStream;
    }

    /// <summary>
    /// Parse a XrefStream into a sequence of calls to the IIndirectObjectRegistry object.
    /// </summary>
    /// <param name="owner">The target of the parsing operation.</param>
    /// <param name="crossRefPdfStream">The XRefStream to parse.</param>
    /// <returns>A task object representing completion of the operation.</returns>
    public static async Task ReadXrefStreamDataAsync(IIndirectObjectRegistry owner, PdfStream crossRefPdfStream)
    {
        var parser = await new XrefStreamParserFactory(crossRefPdfStream, owner).CreateAsync().CA();
        await using var stream = await crossRefPdfStream.StreamContentAsync().CA();
        await parser.ParseAsync(PipeReader.Create(stream)).CA();

    }
}