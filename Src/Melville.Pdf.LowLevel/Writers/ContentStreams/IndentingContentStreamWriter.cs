using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using  Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.ObjectWriters; // for generated code


namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

public partial class IndentingContentStreamWriter : IContentStreamOperations
{

    private readonly PipeWriter destPipe;
    private readonly ContentStreamWriter target;
    private int indentLevel = 0;
    private static readonly byte[] indentText = { 32, 32, 32, 32 };

    public IndentingContentStreamWriter(PipeWriter destPipe)
    {
        this.destPipe = destPipe;
        target = new ContentStreamWriter(destPipe);
    }

    //We make use of the fact that every call to this object should be one line, so we
    // just make accessing the object write the indents.  Calls that increase or decrease
    // the indent are handled as special cases.
    [DelegateTo()]
    private IContentStreamOperations Indented()
    {
        WriteIndentedOperatorPrefix();
        return target;
    }

    private void WriteIndentedOperatorPrefix()
    {
        for (int i = 0; i < indentLevel; i++)
        {
            WriteSingleIndent();
        }
    }

    private void WriteSingleIndent() => destPipe.WriteBytes(indentText);

    public void BeginCompatibilitySection()
    {
        Indented().BeginCompatibilitySection();
        IncreaseIndent();
    }

    public void EndCompatibilitySection()
    {
        DecreaseIndent();
        Indented().EndCompatibilitySection();
    }

    public void BeginTextObject()
    {
        Indented().BeginTextObject();
        IncreaseIndent();
    }

    public void EndTextObject()
    {
        DecreaseIndent();
        Indented().EndTextObject();
    }

    public void BeginMarkedRange(PdfName tag)
    {
        Indented().BeginMarkedRange(tag);
        IncreaseIndent();
    }

    public async ValueTask BeginMarkedRangeAsync(PdfName tag, PdfName dictName)
    {
        await Indented().BeginMarkedRangeAsync(tag, dictName).CA();
        IncreaseIndent();
    }

    public async ValueTask BeginMarkedRangeAsync(PdfName tag, PdfDictionary dictionary)
    {
        await Indented().BeginMarkedRangeAsync(tag, dictionary).CA();
        IncreaseIndent();
    }

    public void EndMarkedRange()
    {
        DecreaseIndent();
        Indented().EndMarkedRange();
    }


    public void SaveGraphicsState()
    {
        Indented().SaveGraphicsState();
        IncreaseIndent();
    }

    public void RestoreGraphicsState()
    {
        DecreaseIndent();
        Indented().RestoreGraphicsState();
    }

    private void IncreaseIndent() => indentLevel++;
    private void DecreaseIndent() => indentLevel--;
}