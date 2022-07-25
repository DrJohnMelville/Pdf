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
        await CrossReferenceStreamParser.ReadXrefStreamData(factory, Source).CA();
        return factory.CreateViewModel();
    }
}

public class XrefParseLogger : IIndirectObjectRegistry
{
    public List<XrefDisplayLine> Lines = new();

    public void RegisterDeletedBlock(int number, int next, int generation) => 
        Lines.Add(new XrefDisplayLine("Deleted", number, next, generation));

    public void RegistedNullObject(int number, int next, int generation)=>
        Lines.Add(new XrefDisplayLine("Null", number, next, generation));
    
    public void RegisterIndirectBlock(int number, long generation, long offset) =>
        Lines.Add(new XrefDisplayLine("Raw", number, generation, offset));

    public void RegisterObjectStreamBlock(
        int number, long referredStreamOrdinal, long positionInStream) =>
        Lines.Add(new XrefDisplayLine("Object Stream", number, referredStreamOrdinal,
            positionInStream));

    public XrefDisplayViewModel CreateViewModel() => new XrefDisplayViewModel(Lines);
}

public record XrefDisplayLine(string Kind, int Number, long First, long Second)
{
}