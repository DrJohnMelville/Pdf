using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal class PdfParsingStack : PostscriptStack<PdfIndirectObject>
{
    private ParsingReader Source { get; }
    private readonly LazyCryptContextBuffer cryptoBuffer;
    public PdfParsingStack(ParsingReader source) : base(0,"")
    {
        Source = source;
        cryptoBuffer = new LazyCryptContextBuffer(source.Owner);
    }

    public void PushMark() =>
        Push(new PdfIndirectObject(PdfParsingCommand.PushMark, default));

    public void CreateArray()
    {
        var ret = new PdfArray(SpanAbove(IdentifyPdfOperator).ToArray());
        var priorSize = Count;
        ClearThrough(IdentifyPdfOperator);
        ClearAfterPop(priorSize);
        Push(ret);
    }

    public void PushRootSignal()
    {
        Debug.Assert(Count == 0);
        Push(new PdfIndirectObject(PdfParsingCommand.ObjOperator, default));
    }

    public bool HasRootSignal() =>
        Count > 0 && this[0].TryGetEmbeddedDirectValue(out var embeddedValue) &&
        embeddedValue.TryGet(out PdfParsingCommand? ppc) &&
        ppc == PdfParsingCommand.ObjOperator;
 
    public void CreateDictionary()
    {
        var stackSpan = SpanAbove(IdentifyPdfOperator);
        if (stackSpan.Length % 2 == 1)
            throw new PdfParseException("Pdf Dictionary much have a even number of elements");

        var dictArray = new KeyValuePair<PdfDirectObject, PdfIndirectObject>[stackSpan.Length / 2];
        var finalPos = 0;
        for (int i = 0; i < stackSpan.Length; i += 2)
        {
            if (!(stackSpan[i].TryGetEmbeddedDirectValue(out var name) && name.IsName))
                throw new PdfParseException("Dictionary keys must be direct values and names");

            var value = stackSpan[i+ 1];
            if (!(value.TryGetEmbeddedDirectValue(out var dirValue) && dirValue.IsNull))
                dictArray[finalPos++] = new(name, value);
        }

        int priorSize = Count;
        ClearThrough(IdentifyPdfOperator);
        ClearAfterPop(priorSize);
        var dataMemory = dictArray.AsMemory(0, finalPos);
        Push(new(PrepareDictionary(dataMemory), (MementoUnion)default));
    }

    private object PrepareDictionary(
        Memory<KeyValuePair<PdfDirectObject, PdfIndirectObject>> dataMemory) =>
        PosibleStreamDeclaration() ?
            dataMemory:new PdfDictionary(dataMemory);

    private bool PosibleStreamDeclaration() => HasRootSignal()&&Count== 1;

    public void ObjOperator()
    {
        Debug.Assert(Count == 3);
        SetObjectNumber();
        this.ClearAfterPop(3);
    }

    private void SetObjectNumber()
    {
        var generation = ForcePopInteger();
        var objectNumber = ForcePopInteger();
        cryptoBuffer.SetCurrentObject(objectNumber, generation);
    }

    private int ForcePopInteger() =>
        Pop().TryGetEmbeddedDirectValue(out var dirVal) && dirVal.TryGet(out int ret)
            ? ret
            : throw new PdfParseException("Direct valued integer expected");

    public void CreateReference()
    {
        var generation = PopNumber();
        var objNumber = PopNumber();
        Push(Source.Owner.NewIndirectResolver.CreateReference(objNumber, generation));
    }

    private int PopNumber() =>
        Pop().TryGetEmbeddedDirectValue(out var dv) && dv.TryGet(out int num)
            ? num
            : throw new PdfParseException("Expected two direct numbers prior to R operator");

    public async ValueTask StreamOperatorAsync()
    {
        Debug.Assert(Count == 2);
        Debug.Assert(Peek().TryGetEmbeddedDirectValue(
            out Memory<KeyValuePair<PdfDirectObject, PdfIndirectObject>> _));
        AdvancePastWhiteSpace(await Source.Reader.ReadAtLeastAsync(10).CA());
        Pop().TryGetEmbeddedDirectValue(out var dv);
        Pop();
        Push(new PdfStream(new  PdfFileStreamSource(
                Source.Reader.GlobalPosition, Source.Owner, CryptoContext()), 
                dv.Get<Memory<KeyValuePair<PdfDirectObject, PdfIndirectObject>>>()));
    }

    private void AdvancePastWhiteSpace(ReadResult ca)
    {
        var reader = new SequenceReader<byte>(ca.Buffer);
        TryEatWhiteSpace(ref reader);
        TryAdvancePastChar(ref reader, (byte)'\r');
        TryAdvancePastChar(ref reader, (byte)'\n');
        Source.Reader.AdvanceTo(reader.Position);
    }

    private static void TryAdvancePastChar(ref SequenceReader<byte> reader, byte soughtCharacter)
    {
        if (reader.TryPeek(out var c1) && c1 == soughtCharacter)
        {
            reader.Advance(1);
        }
    }

    private void TryEatWhiteSpace(ref SequenceReader<byte> reader)
    {
        while (reader.TryPeek(out byte character) && !CharacterClassifier.IsLineEndChar(character) &&
               CharacterClassifier.IsWhitespace(character))
        {
            reader.Advance(1);
        }
    }

    public void EndObject()
    {
        Debug.Assert(Count == 2);
        var result = Pop();
        cryptoBuffer.ClearObject();
        Pop();
        ClearAfterPop(2);
        Push(result.TryGetEmbeddedDirectValue(
            out Memory<KeyValuePair<PdfDirectObject, PdfIndirectObject>> mem)
            ? new PdfDictionary(mem)
            : result);
    }

    
    public void EndStreamOperator()
    {
    }

    public IObjectCryptContext CryptoContext() => cryptoBuffer.GetContext();

    private bool IdentifyPdfOperator(PdfIndirectObject i) => i.IsPdfParsingOperation();
}