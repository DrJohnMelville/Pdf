using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal class PdfObjectWriter
{
    private readonly PipeWriter target;
    private IDocumentCryptContext encryptor;
    private (int ObjectNum,int Generation)? currentIndirectObject = null;

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

    public void Write(PdfDirectValue item) =>
        // Assert that item is not a Stream
        throw new NotFiniteNumberException();

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
                default: (objNum, generation); ;
        return this.WriteObjectDefinition(objNum, generation, value);
    }

    public ValueTask WriteStreamAsync(PdfValueStream stream) => 
        StreamWriter.WriteAsync(this, stream, CreateEncryptor());

    private IObjectCryptContext CreateEncryptor() =>
        !currentIndirectObject.HasValue ? NullSecurityHandler.Instance : 
            encryptor.ContextForObject(currentIndirectObject.Value.ObjectNum, 
                currentIndirectObject.Value.Generation);


    /*
    // this is an unusual situation where the methods have to not be named async to
    //implement the interface which is defined over a valueType
    public override ValueTask<FlushResult> Visit(PdfTokenValues item) => 
        TokenValueWriter.WriteAsync(target, item);
    public override ValueTask<FlushResult> Visit(PdfString item) => 
        StringWriter.WriteAsync(target, item, CreateEncryptor());
    public override ValueTask<FlushResult> Visit(PdfInteger item) =>
        IntegerWriter.WriteAndFlushAsync(target, item.IntValue);
    public override ValueTask<FlushResult> Visit(PdfDouble item) =>
        DoubleWriter.WriteAsync(target, item.DoubleValue);

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