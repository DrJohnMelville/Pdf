using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

/// <summary>
/// A helper method to convert postscript names to pdf name
/// </summary>
public static class NameMapper
{
    /// <summary>
    /// Create a PDF Name from a postscript name.
    /// </summary>
    /// <param name="value">postscript name to make into a pdf name</param>
    /// <returns>A pdf name with the same value.</returns>
    /// <exception cref="PdfParseException">If the PostscriptValue is not a literal name</exception>
    public static PdfDirectObject AsPdfName(this in PostscriptValue value)
    {
        if (!value.IsLiteralName) throw new PdfParseException("NameExpected");
        return new PdfDirectObject(value.ValueStrategy, value.Memento);
    }
}