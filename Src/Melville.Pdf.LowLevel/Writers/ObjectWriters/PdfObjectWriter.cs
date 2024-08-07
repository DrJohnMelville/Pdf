﻿using System;
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

    public void Write(PdfIndirectObject item)
    {
        if (item.TryGetEmbeddedDirectValue(out var directValue))
            Write(directValue);
        else
        { 
            this.WriteObjectReference((int)item.Memento.UInt64s[0],(int)item.Memento.UInt64s[1]);
        }
    }

    public ValueTask WriteAsync(PdfDirectObject item)
    {
        if (item.TryGet(out PdfStream? stream))
            return WriteStreamAsync(stream);
        Write(item);
        return ValueTask.CompletedTask;
    }

    public void Write(PdfDirectObject item)
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
            case var x when x.TryGet(out PdfArray? arr):
                ArrayWriter.WriteArray(this, arr);
                break;
            #if DEBUG
            case var x when x.TryGet(out PdfStream? dict):
                throw new InvalidOperationException("Cannot write a stream from this method");
            #endif
            case var x when x.TryGet(out PdfDictionary? dict):
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

    public async ValueTask CopyFromStreamAsync(Stream stream)
    {
        await target.FlushAsync().CA();
        await stream.CopyToAsync(target).CA();
    }

    public ValueTask WriteTopLevelDeclarationAsync(
        int objNum, int generation, PdfDirectObject value)
    {
        currentIndirectObject =
            value.TryGet(out PdfDictionary? pvd) && encryptor.BlockEncryption(pvd) ? (-1,-1): (objNum, generation); 
        return this.WriteObjectDefinitionAsync(objNum, generation, value);
    }

    public ValueTask WriteStreamAsync(PdfStream stream) => 
        StreamWriter.WriteAsync(this, stream, CreateEncryptor());

    private IObjectCryptContext CreateEncryptor()
    {
        Debug.Assert(currentIndirectObject.ObjectNum != 0);
        return currentIndirectObject.ObjectNum < 0
            ? NullSecurityHandler.Instance
            : encryptor.ContextForObject(currentIndirectObject.ObjectNum,
                currentIndirectObject.Generation);
    }
}