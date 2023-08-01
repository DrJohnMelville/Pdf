using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.INPC;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

// for generated code


namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

/// <summary>
/// This contentstream writer is a thin wrapper around a ContentStreamWriter that it
/// creates in the constructor.  This class adds whitespace to pretty print and indent the
/// content stream.
/// </summary>
public partial class IndentingContentStreamWriter : IContentStreamOperations
{
    private readonly PipeWriter destPipe;
    private readonly ContentStreamWriter target;
    private int indentLevel = 0;
    private static ReadOnlySpan<byte> IndentText => "    "u8;

    /// <summary>
    /// Create an IndentingContentStreamWriter
    /// </summary>
    /// <param name="destPipe">The pipe to receive the output content stream</param>
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

    private void WriteSingleIndent() => destPipe.WriteBytes(IndentText);

    /// <inheritdoc />
    public void BeginCompatibilitySection()
    {
        Indented().BeginCompatibilitySection();
        IncreaseIndent();
    }

    /// <inheritdoc />
    public void EndCompatibilitySection()
    {
        DecreaseIndent();
        Indented().EndCompatibilitySection();
    }

    /// <inheritdoc />
    public void BeginTextObject()
    {
        Indented().BeginTextObject();
        IncreaseIndent();
    }

    /// <inheritdoc />
    public void EndTextObject()
    {
        DecreaseIndent();
        Indented().EndTextObject();
    }

    /// <inheritdoc />
    public void BeginMarkedRange(PdfDirectValue tag)
    {
        Indented().BeginMarkedRange(tag);
        IncreaseIndent();
    }

    /// <inheritdoc />
    public async ValueTask BeginMarkedRangeAsync(PdfDirectValue tag, PdfDirectValue dictName)
    {
        await Indented().BeginMarkedRangeAsync(tag, dictName).CA();
        IncreaseIndent();
    }

    /// <inheritdoc />
    public async ValueTask BeginMarkedRangeAsync(PdfDirectValue tag, PdfValueDictionary dictionary)
    {
        await Indented().BeginMarkedRangeAsync(tag, dictionary).CA();
        IncreaseIndent();
    }

    /// <inheritdoc />
    public void EndMarkedRange()
    {
        DecreaseIndent();
        Indented().EndMarkedRange();
    }


    /// <inheritdoc />
    public void SaveGraphicsState()
    {
        Indented().SaveGraphicsState();
        IncreaseIndent();
    }

    /// <inheritdoc />
    public void RestoreGraphicsState()
    {
        DecreaseIndent();
        Indented().RestoreGraphicsState();
    }

    private void IncreaseIndent() => indentLevel++;
    private void DecreaseIndent() => indentLevel--;
}