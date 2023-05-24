using System;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.CryptContexts;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Visitors;

namespace Melville.Pdf.LowLevel.Writers.ObjectWriters;

internal class PdfObjectWriter: RecursiveDescentVisitor<ValueTask<FlushResult>>
{
    private readonly PipeWriter target;
    private IDocumentCryptContext encryptor;
    private PdfIndirectObject? currentIndirectObject = null;

    public PdfObjectWriter(PipeWriter target) : this(target, NullSecurityHandler.Instance)
    {
    }

    internal PdfObjectWriter(PipeWriter target, IDocumentCryptContext encryptor) : base()
    {
        this.target = target;
        this.encryptor = encryptor;
    }

    // this is an unusual situation where the methods have to not be named async to
    //implement the interface which is defined over a valueType
#pragma warning disable Arch004
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
#pragma warning restore Arch004

    private IObjectCryptContext CreateEncryptor() =>
        currentIndirectObject == null ? NullSecurityHandler.Instance : 
            encryptor.ContextForObject(currentIndirectObject.ObjectNumber, currentIndirectObject.GenerationNumber);

}