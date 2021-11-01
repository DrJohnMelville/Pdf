using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils;

public static class LexerTestUtil
{
    public static PipeReader AsPipeReader(this string data)=>
        PipeReader.Create(new MemoryStream(data.AsExtendedAsciiBytes()));

    public static ReadOnlySequence<byte> AsReadOnlySequence(this string data) =>
        new(data.AsExtendedAsciiBytes());

    public static SequenceReader<byte> AsSequenceReader(this string data) =>
        new(data.AsReadOnlySequence());
}