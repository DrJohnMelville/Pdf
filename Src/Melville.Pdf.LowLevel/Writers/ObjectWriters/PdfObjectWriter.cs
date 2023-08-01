using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Postscript.Interpreter.Values;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;
using PdfIndirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfIndirectValue;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal class PdfObjectWriter
{
    private readonly PipeWriter target;
    private IDocumentCryptContext encryptor;
    private (int ObjectNum,int Generation) currentIndirectObject = (-1,-1);

    public PdfObjectWriter(PipeWriter target) : this(target, NullSecurityHandler.Instance)
    {
    }

    internal PdfObjectWriter(PipeWriter target, IDocumentCryptContext encryptor) : base()
    {
        this.target = target;
        this.encryptor = encryptor;
    }

    public void Write(PdfIndirectValue item)
    {
        if (item.TryGetEmbeddedDirectValue(out var directValue))
            Write(directValue);
        else
        { 
            this.WriteObjectReference((int)item.Memento.UInt64s[0],(int)item.Memento.UInt64s[1]);
        }
    }

    public void Write(PdfDirectValue item)
    {
        switch (item)
        {
            case {IsInteger:true}:
                IntegerWriter.Write(target, item.Get<long>());
                break;
            case {IsDouble:true}:
                DoubleWriter.Write(target, item.Get<double>());
                break;
            case {IsBool:true}:
                Write(item.Get<bool>()?"true"u8:"false"u8);
                break;
            case {IsNull:true}:
                Write("null"u8);
                break;
            case {IsName:true}:
                NameWriter.Write(target, item);
                break;
            case {IsString:true}:
                StringWriter.Write(target, item.Get<StringSpanSource>().GetSpan(), 
                    CreateEncryptor());
                break;
            case var x when x.TryGet(out PdfValueArray arr):
                ArrayWriter.WriteArray(this, arr);
                break;
            #if DEBUG
            case var x when x.TryGet(out PdfValueStream dict):
                throw new InvalidOperationException("Cannot write a stream from this method");
            #endif
            case var x when x.TryGet(out PdfValueDictionary dict):
                DictionaryWriter.Write(this, dict.RawItems);
                break;
            default:
                throw new NotImplementedException($"Cannot write value {item}");
        }
    }

    public void Write(in ReadOnlySpan<byte> literal)
    {
        literal.CopyTo(target.GetSpan(literal.Length));
        target.Advance(literal.Length);
    }

    public async ValueTask CopyFromStream(Stream stream)
    {
        await target.FlushAsync().CA();
        await stream.CopyToAsync(target).CA();
    }

    public ValueTask WriteTopLevelDeclarationAsync(
        int objNum, int generation, PdfDirectValue value)
    {
        currentIndirectObject =
            value.TryGet(out PdfValueDictionary? pvd) && encryptor.BlockEncryption(pvd) ? 
                (-1,-1): (objNum, generation); ;
        return this.WriteObjectDefinition(objNum, generation, value);
    }

    public ValueTask WriteStreamAsync(PdfValueStream stream) => 
        StreamWriter.WriteAsync(this, stream, CreateEncryptor());

    private IObjectCryptContext CreateEncryptor()
    {
        Debug.Assert(currentIndirectObject.ObjectNum != 0);
        return currentIndirectObject.ObjectNum < 0
            ? NullSecurityHandler.Instance
            : encryptor.ContextForObject(currentIndirectObject.ObjectNum,
                currentIndirectObject.Generation);
    }

#warning -- get rid of this comment when all the tests pass
    /*
    // this is an unusual situation where the methods have to not be named async to
    //implement the interface which is defined over a valueType
    public override ValueTask<FlushResult> Visit(PdfTokenValues item) => 
        TokenValueWriter.WriteAsync(target, item);

    public override ValueTask<FlushResult> Visit(PdfString item) => 
        StringWriter.WriteAsync(target, item, CreateEncryptor());

    public override ValueTask<FlushResult> VisitTopLevelObject(PdfIndirectObject item)
    { 
        if (!item.HasValue())
            throw new InvalidOperationException("Promised indirect objects must be assigned befor writing the file");
        currentIndirectObject = encryptor.BlockEncryption(item)?null: item;
        return IndirectObjectWriter.WriteObjectDefinitionAsync(target, item, this);
    }

    public override ValueTask<FlushResult> Visit(PdfIndirectObject item) =>
        IndirectObjectWriter.WriteObjectReferenceAsync(target, item);
    public override ValueTask<FlushResult> Visit(PdfName item) =>
        NameWriter.WriteAsync(target, item);
    public override ValueTask<FlushResult> Visit(PdfArray item) => 
        ArrayWriter.WriteAsync(target, this, item.RawItems);
    public override ValueTask<FlushResult> Visit(PdfDictionary item)
    {
        if (encryptor.BlockEncryption(item))
        {
            currentIndirectObject = null;
        }
        return DictionaryWriter.WriteAsync(target, this, item.RawItems);
    }

    public override ValueTask<FlushResult> Visit(PdfStream item) =>
        StreamWriter.WriteAsync(target, this, item, CreateEncryptor());

    private IObjectCryptContext CreateEncryptor() =>
        currentIndirectObject == null ? NullSecurityHandler.Instance : 
            encryptor.ContextForObject(currentIndirectObject.ObjectNumber, currentIndirectObject.GenerationNumber);
*/

}