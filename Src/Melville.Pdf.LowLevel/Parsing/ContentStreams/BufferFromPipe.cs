using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Primitives.PipeReaderWithPositions;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public readonly struct BufferFromPipe
{
    private readonly PipeReader reader;
    private readonly ReadResult result;
    private readonly SequencePosition startAt;
    public ReadOnlySequence<byte> Buffer => result.Buffer;

    public static async ValueTask<BufferFromPipe> Create(PipeReader reader)
    {
        var readResult = await reader.ReadAsync();
        return new BufferFromPipe(reader, readResult, readResult.Buffer.Start);
    }

    public ValueTask<BufferFromPipe> Refresh() => Create(reader);
    public ValueTask<BufferFromPipe> InvalidateAndRefresh()
    {
        NeedMoreBytes();
        return Refresh();
    }


    private BufferFromPipe(PipeReader reader, ReadResult result, SequencePosition startAt)
    {
        this.reader = reader;
        this.result = result;
        this.startAt = startAt;
    }

    public bool Done => result.IsCompleted;
    public SequenceReader<byte> CreateReader() => new (result.Buffer.Slice(startAt));
    public void Consume(SequencePosition pos) => reader.AdvanceTo(pos);

    public bool NeedMoreBytes()
    {
        reader.AdvanceTo(result.Buffer.Start, result.Buffer.End);
        return false;
    }

    public BufferFromPipe WithStartingPosition(SequencePosition sp) => new(reader, result, sp);

    public bool ConsumeIfSucceeded(bool succeed, ref SequenceReader<byte> reader)
    {
        if (succeed)
        {
            Consume(reader.Position);
            return true;
        }
        NeedMoreBytes();
        return false;
    }

    public IParsingReader CreateParsingReader()
    {
        reader.AdvanceTo(startAt);
        return new ContentStreamParsingReader(reader);
    }
}

public class ContentStreamParsingReader : IParsingReader
{
    public void Dispose()
    {
    }

    public IPdfObjectParser RootObjectParser => PdfParserParts.ContentStreamComposite;

    public IIndirectObjectResolver IndirectResolver => throw new NotSupportedException();

    public ParsingFileOwner Owner => throw new NotSupportedException();

    public IPipeReaderWithPosition Reader {get;}

    public IObjectCryptContext ObjectCryptContext() => NullSecurityHandler.Instance;

    public ContentStreamParsingReader(PipeReader reader)
    {
        Reader = new PipeReaderWithPosition(reader, 0);
    }
}