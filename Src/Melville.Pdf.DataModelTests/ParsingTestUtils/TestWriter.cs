using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Parsing.Streams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Writers.ObjectWriters;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public class TestWriter
{
    private readonly MultiBufferStream target = new();
    public PipeWriter Writer { get; }

    public TestWriter()
    {
        Writer = PipeWriter.Create(target);
    }

    public string Result() => ExtendedAsciiEncoding.ExtendedAsciiString(
        target.CreateReader().ReadToArray());
}

public static class TestWriterOperations
{
    public static ValueTask<string> WriteToStringAsync(this PdfDirectObject obj) =>
        ((PdfIndirectObject)obj).WriteToStringAsync();
    public static async ValueTask<string> WriteToStringAsync(this PdfIndirectObject obj)
    {
        var writer = new TestWriter();
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
        var writer = new TestWriter();
        var objWriter = new PdfObjectWriter(writer.Writer);
        await objWriter.WriteStreamAsync(pdfStream);
        await writer.Writer.FlushAsync();
        return writer.Result();
    }
}