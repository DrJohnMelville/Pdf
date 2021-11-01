using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.FileSystem;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
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
    public static async ValueTask<string> WriteToStringAsync(this PdfObject obj)
    {
        var writer = new TestWriter();
        var objWriter = new PdfObjectWriter(writer.Writer);
        await obj.Visit(objWriter);
        return writer.Result();
    }
}