using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

public static class CrossReferenceStreamParser
{
    public static async Task<PdfDictionary> Read(ParsingFileOwner owner, long offset)
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

    public static async Task ReadXrefStreamData(IIndirectObjectRegistry owner, PdfStream crossRefPdfStream)
    {
        var parser = await new XrefStreamParserFactory(crossRefPdfStream, owner).Create().CA();
        await parser.Parse(PipeReader.Create(await crossRefPdfStream.StreamContentAsync().CA())).CA();

    }
}