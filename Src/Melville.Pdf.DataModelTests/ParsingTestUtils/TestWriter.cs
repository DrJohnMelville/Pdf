using System;
using System.IO;
using System.IO.Pipelines;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.DataModelTests.ParsingTestUtils
{
    public class TestWriter
    {
        private readonly MemoryStream target = new();
        public PipeWriter Writer { get; }

        public TestWriter()
        {
            Writer = PipeWriter.Create(target);
        }

        public string Result() => ExtendedAsciiEncoding.ExtendedAsciiString(
            target.GetBuffer().AsSpan()[..(int)target.Length]);
    }
}