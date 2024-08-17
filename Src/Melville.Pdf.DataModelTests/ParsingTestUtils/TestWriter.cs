using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public class TestWriter : IDisposable
{
    private readonly IWritableMultiplexSource target = WritableBuffer.Create();
    public PipeWriter Writer { get; }

    public TestWriter()
    {
        Writer = target.WritingPipe();
    }

    public string Result()
    {
        using var readFrom = target.ReadFrom(0);
        return readFrom.ReadToArray().ExtendedAsciiString();
    }

    public void Dispose()
    {
        Writer.Complete();
        target.Dispose();
    }
}

public static class TestWriterOperations
{
    public static ValueTask<string> WriteToStringAsync(this PdfDirectObject obj) =>
        ((PdfIndirectObject)obj).WriteToStringAsync();

    public static async ValueTask<string> WriteToStringAsync(this PdfIndirectObject obj)
    {
        using var writer = new TestWriter();
        var objWriter = new PdfObjectWriter(writer.Writer);
        objWriter.Write(obj);
        await writer.Writer.FlushAsync();
        return writer.Result();
    }

    public static async ValueTask<string> WriteStreamToStringAsync(this PdfDirectObject obj)
    {
        var pdfValueStream = obj.Get<PdfStream>();
        return await WriteStreamToStringAsync(pdfValueStream);
    }

    public static async Task<string> WriteStreamToStringAsync(this PdfStream pdfStream)
    {
        using var writer = new TestWriter();
        var objWriter = new PdfObjectWriter(writer.Writer);
        await objWriter.WriteStreamAsync(pdfStream);
        await writer.Writer.FlushAsync();
        return writer.Result();
    }
}