using System.IO;
using System.IO.Pipelines;
using Melville.INPC;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.ObjectRentals;
using Melville.Parsing.PipeReaders;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ParserContext;

internal partial class ParsingReader
{
    [FromConstructor] public ParsingFileOwner Owner { get; }
    [FromConstructor] public IByteSource Reader { get; }

    public ParsingReader(ParsingFileOwner owner, Stream input, long lastSeek) :
        this(owner, new ByteSourceWithGlobalPosition(
            ObjectPool<ReusableStreamPipeReader>.Shared.Rent()
                .WithParameters(input, true)
            , lastSeek))
    {
    }
}