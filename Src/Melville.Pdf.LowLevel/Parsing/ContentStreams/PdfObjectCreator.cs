using System.Diagnostics;
using Melville.INPC;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Values;
using Melville.Postscript.Interpreter.Values.Numbers;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

internal readonly partial struct PdfObjectCreator
{
    [FromConstructor]private readonly OperandStack stack;
    [FromConstructor] private readonly DictionaryTranslator translator;

    public DictionaryBuilder PopDictionaryBuilderFromStack()
    {
        var builder = new DictionaryBuilder();
        return PopDictionaryBuilderFromStack(builder);
    }

    public DictionaryBuilder PopDictionaryBuilderFromStack(DictionaryBuilder builder)
    {
        while (stack.Count > 1 && stack.Peek() is { IsMark: false })
        {
            var item = translator.PopNonname(this);
            var key = translator.PopName(this);
            builder.WithItem(key, item);
        }

        PopMarkObject();
        return builder;
    }

    public PdfDirectObject PopPdfObjectFromStack()
    {
        var item = stack.Pop();
        if (IsArrayTopMarker(item)) return PopInlineArray();
        return PostscriptToPdfObject(item);
    }

    private static bool IsArrayTopMarker(PostscriptValue item) =>
        item.TryGet(out ArrayTopMarker? _);

    private PdfDirectObject PopInlineArray()
    {
        var ret = new PdfIndirectObject[stack.CountToMark()];
        for (int i = ret.Length - 1; i >= 0; i--)
        {
            ret[i] = translator.PopNonname(this);
        }

        PopMarkObject();
        return new(new PdfArray(ret), default);
    }

    private void PopMarkObject()
    {
        var mark = stack.Pop();
        Debug.Assert(mark.IsMark);
    }

    private PdfDirectObject PostscriptToPdfObject(PostscriptValue item) => item switch
    {
        { IsInteger: true } or
            { IsDouble: true } or
            { IsString: true } or
            { IsBoolean: true } or
            { IsLiteralName: true } => MapSimpleValue(item),
        var x when x.TryGet(out PdfDictionary? innerDictionary) => innerDictionary,
        var x when x.TryGet(out PdfArray? innerArray) => innerArray,
        _ => throw new PdfParseException("Cannot convert PostScriptToPdfObject")
    };

    private static PdfDirectObject MapSimpleValue(in PostscriptValue source)
    {
        Debug.Assert(source.ValueStrategy is PostscriptString or PostscriptDouble or PostscriptBoolean
            or PostscriptInteger);
        return new PdfDirectObject(source.ValueStrategy, source.Memento);
    }
}