using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;
using StringWriter = Melville.Pdf.LowLevel.Writers.ObjectWriters.StringWriter;

namespace Melville.Pdf.LowLevel.Writers.ContentStreams;

internal readonly struct ContentStreamPipeWriter
{
    private readonly PipeWriter destPipe;

    public ContentStreamPipeWriter(PipeWriter destPipe)
    {
        this.destPipe = destPipe;
    }

    public void WriteOperator(in ReadOnlySpan<byte> name)
    {
        WriteBytes(name);
        WriteNewLine();
    }

    public void WriteBytes(ReadOnlySpan<byte> name) => destPipe.WriteBytes(name);

    public void WriteOperator(in ReadOnlySpan<byte> op, PdfDirectValue name)
    {
        WriteName(name);
        WriteOperator(op);
    }
    public void WriteOperator(in ReadOnlySpan<byte> op, params PdfDirectValue[] names)
    {
        foreach (var name in names)
        {
            WriteName(name);
        }
        WriteOperator(op);
    }

    public void WriteOperator(in ReadOnlySpan<byte> operation, double value)
    {
        WriteDoubleAndSpace(value);
        WriteOperator(operation);
    }

    public void WriteOperator(in ReadOnlySpan<byte> operation, params double[] values) =>
        WriteOperator(operation, new ReadOnlySpan<double>(values));

    public void WriteOperator(in ReadOnlySpan<byte> operation, in ReadOnlySpan<double> values)
    {
        WriteDoubleSpan(values);
        WriteOperator(operation);
    }
    public void WriteOperator(in ReadOnlySpan<byte> operation, ReadOnlySpan<byte> stringValue)
    {
        WriteString(stringValue);
        WriteOperator(operation);
    }

    public void WriteString(in ReadOnlySpan<byte> stringValue) => 
        StringWriter.WriteSpanAsString(destPipe, stringValue);

    public void WriteDoubleSpan(in ReadOnlySpan<double> values)
    {
        foreach (var value in values)
        {
            WriteDoubleAndSpace(value);
        }
    }

    public void WriteDoubleAndSpace(double d)
    {
        WriteDouble(d);
        WriteSpace();
    }

    public void WriteDouble(double d) => destPipe.Advance(DoubleWriter.Write(d, destPipe.GetSpan(25)));

    public void WriteName(PdfDirectValue name)
    {
        NameWriter.WriteWithoutlush(destPipe, name);
        WriteSpace();
    }

    public void WriteChar(char c) => destPipe.WriteByte((byte)c);
    public void WriteNewLine() => WriteChar('\n');


    public void WriteSpace() => WriteChar(' ');

    public void WriteDoubleArray(in ReadOnlySpan<double> dashArray)
    {
        WriteChar('[');
        if (dashArray.Length > 0)
        { 
            WriteDoublesWithSpaces(dashArray[..^1]);
            WriteDouble(dashArray[^1]);
        }
        WriteChar(']');
        WriteSpace();
    }
    private void WriteDoublesWithSpaces(in ReadOnlySpan<double> values)
    {
        foreach (var value in values)
        {
            WriteDoubleAndSpace(value);
        }
    }

    public void WriteOperator(in ReadOnlySpan<byte> operation, PdfValueDictionary dictionary)
    {
        WriteDictionary(dictionary);
        WriteOperator(operation);
    }

    public void WriteDictionary(PdfValueDictionary dict) => 
        new PdfObjectWriter(destPipe).Write(dict);

    public void WriteInlineImageDict(PdfValueDictionary dict) =>
        DictionaryWriter.WriteInlineImageDict(new PdfObjectWriter(destPipe), dict.RawItems);

    public Task WriteStreamContentAsync(Stream str) => str.CopyToAsync(destPipe);

    public void WriteLiteral(string contentStream)
    {
        var data = destPipe.GetSpan(contentStream.Length);
        ExtendedAsciiEncoding.EncodeToSpan(contentStream, data);
        destPipe.Advance(contentStream.Length);
    }
}