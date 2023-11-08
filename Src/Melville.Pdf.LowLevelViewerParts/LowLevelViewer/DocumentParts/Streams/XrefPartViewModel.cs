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

    protected override async ValueTask AddFormatsAsync(List<StreamDisplayFormat> fmts)
    {
        await base.AddFormatsAsync(fmts);
        fmts.Insert(0, new StreamDisplayFormat("Xref", ParseXrefAsync));
    }

    private async ValueTask<object> ParseXrefAsync(PdfStream arg)
    {
        var factory = new XrefParseLogger();
        await CrossReferenceStreamParser.ReadXrefStreamDataAsync(factory, Source).CA();
        return factory.CreateViewModel();
    }
}

public class XrefParseLogger : IIndirectObjectRegistry
{
    public List<XrefDisplayLine> Lines = new();
    
    public XrefDisplayViewModel CreateViewModel() => new XrefDisplayViewModel(Lines);
    public void RegisterDeletedBlock(int number, int next, int generation)=>
        Lines.Add(new XrefDisplayLine("Deleted", number, next, generation));

    public void RegisterNullObject(int number, int next, int generation)=>
        Lines.Add(new XrefDisplayLine("Null", number, next, generation));

    public void RegisterIndirectBlock(int number, int generation, long offset)=>
        Lines.Add(new XrefDisplayLine("Raw", number, generation, offset));

    public void RegisterObjectStreamBlock(
        int number, int referredStreamOrdinal, int positionInStream)=>
        Lines.Add(new XrefDisplayLine("Object Stream", number, referredStreamOrdinal,
            positionInStream));
}

public record XrefDisplayLine(string Kind, int Number, long First, long Second)
{
}