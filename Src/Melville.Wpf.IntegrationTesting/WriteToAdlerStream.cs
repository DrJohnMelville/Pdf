using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.Streams.Bases;
using Melville.Pdf.LowLevel.Filters.FlateFilters;

namespace Melville.Wpf.IntegrationTesting;

public class WriteToAdlerStream: DefaultBaseStream
{
    public Adler32Computer Computer { get; } = new();
    
    public WriteToAdlerStream() : base(false, true, false)
    {
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        Position += buffer.Length;
        Computer.AddData(buffer);
    }
}