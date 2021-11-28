using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Melville.Parsing.Streams.Bases;

namespace Melville.Pdf.LowLevel.Filters.StreamFilters;

public class SequentialReadFilterStream: DefaultBaseStream
{
    public SequentialReadFilterStream() : base(true, false, false)
    {
    }
    
}