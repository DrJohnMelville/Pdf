using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.FileParsers;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts.Streams;

public class XrefDisplayViewModel
{
    public IReadOnlyList<XrefDisplayLine> Lines { get; }

    public XrefDisplayViewModel(IReadOnlyList<XrefDisplayLine> lines)
    {
        Lines = lines;
    }
}

public class XrefPartViewModel : StreamPartViewModel
{
    public XrefPartViewModel(string title, IReadOnlyList<DocumentPart> children, PdfStream source) : 
        base(title, children, source)
    {
    }

    protected override async ValueTask AddFormats(List<StreamDisplayFormat> fmts)
    {
        await base.AddFormats(fmts);
        fmts.Add(new StreamDisplayFormat("Xref", ParseXref));
    }

    private async ValueTask<object> ParseXref(PdfStream arg)
    {
        var factory = new XrefParseLogger();
        await CrossReferenceStreamParser.ReadXrefStreamDataAsync(factory, Source).CA();
        return factory.CreateViewModel();
    }
}

public class XrefParseLogger : IIndirectObjectRegistry
{
    public List<XrefDisplayLine> Lines = new();

    public void RegisterDeletedBlock(int number, ulong next, ulong generation) =>
        Lines.Add(new XrefDisplayLine("Deleted", number, (long)next, (long)generation));

    public void RegisterNullObject(int number, ulong next, ulong generation)=>
        Lines.Add(new XrefDisplayLine("Null", number, (long)next, (long)generation));
    
    public void RegisterIndirectBlock(int number, ulong generation, ulong offset) =>
        Lines.Add(new XrefDisplayLine("Raw", number, (long)generation, (long)offset));

    public void RegisterObjectStreamBlock(
        int number, ulong referredStreamOrdinal, ulong positionInStream) =>
        Lines.Add(new XrefDisplayLine("Object Stream", number, (long)referredStreamOrdinal,
            (long)positionInStream));

    public XrefDisplayViewModel CreateViewModel() => new XrefDisplayViewModel(Lines);
}

public record XrefDisplayLine(string Kind, int Number, long First, long Second)
{
}