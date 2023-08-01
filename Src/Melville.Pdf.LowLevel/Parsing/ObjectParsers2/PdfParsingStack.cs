using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.InterpreterState;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers2;

internal class PdfParsingStack : PostscriptStack<PdfIndirectValue>
{
    private IParsingReader Source { get; }
    private readonly LazyCryptContextBuffer cryptoBuffer;
    public PdfParsingStack(IParsingReader source) : base(0,"")
    {
        Source = source;
        cryptoBuffer = new LazyCryptContextBuffer(source.Owner);
    }

    public void PushMark() =>
        Push(new PdfIndirectValue(PdfParsingCommand.PushMark, default));

    public void CreateArray()
    {
        var ret = new PdfValueArray(SpanAbove(IdentifyPdfOperator).ToArray());
        var priorSize = Count;
        ClearThrough(IdentifyPdfOperator);
        ClearAfterPop(priorSize);
        Push(ret);
    }

    public void PushRootSignal()
    {
        Debug.Assert(Count == 0);
        Push(new PdfIndirectValue(PdfParsingCommand.ObjOperator, default));
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

        var dictArray = new KeyValuePair<PdfDirectValue, PdfIndirectValue>[stackSpan.Length / 2];
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
        Push(new(PrepareDictionary(dataMemory), default));
    }

    private object PrepareDictionary(
        Memory<KeyValuePair<PdfDirectValue, PdfIndirectValue>> dataMemory) =>
        PosibleStreamDeclaration() ?
            dataMemory:new PdfValueDictionary(dataMemory);

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

    public async ValueTask StreamOperator()
    {
        Debug.Assert(Count == 2);
        Debug.Assert(Peek().TryGetEmbeddedDirectValue(
            out Memory<KeyValuePair<PdfDirectValue, PdfIndirectValue>> _));
        AdvavancePastWhiteSpace(await Source.Reader.ReadMinAsync(2).CA());
        Pop().TryGetEmbeddedDirectValue(out var dv);
        Pop();
        Push(new PdfValueStream(new  PdfFileStreamSource(
                Source.Reader.GlobalPosition, Source.Owner, CryptoContext()), 
                dv.Get<Memory<KeyValuePair<PdfDirectValue, PdfIndirectValue>>>()));
    }

    private void AdvavancePastWhiteSpace(ReadResult ca)
    {
        var reader = new SequenceReader<byte>(ca.Buffer);
        for (int i = 0; i < 2; i++)
        {
            if (reader.TryPeek(out var character) &&
                CharacterClassifier.IsLineEndChar(character))
            {
                reader.Advance(1);
            }
        }
        Source.Reader.AdvanceTo(reader.Position);
    }

    public void EndObject()
    {
        Debug.Assert(Count == 2);
        var result = Pop();
        cryptoBuffer.ClearObject();
        Pop();
        ClearAfterPop(2);
        Push(result.TryGetEmbeddedDirectValue(
            out Memory<KeyValuePair<PdfDirectValue, PdfIndirectValue>> mem)
            ? new PdfValueDictionary(mem)
            : result);
    }

    
    public void EndStreamOperator()
    {
    }

    public IObjectCryptContext CryptoContext() => cryptoBuffer.GetContext();

    private bool IdentifyPdfOperator(PdfIndirectValue i) => i.IsPdfParsingOperation();
}