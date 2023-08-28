using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

internal static class NameMapper
{
    public static PdfDirectObject AsPdfName(this in PostscriptValue value)
    {
        if (!value.IsLiteralName) throw new PdfParseException("NameExpected");
        return new PdfDirectObject(value.ValueStrategy, value.Memento);
    }
}