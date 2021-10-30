using System;
using System.Buffers;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public class ContentStreamParser
{
    private readonly ContentStreamContext target;

    public ContentStreamParser(IContentStreamOperations target)
    {
        this.target = new ContentStreamContext(target);
    }

    public async ValueTask Parse(PipeReader source)
    {
        ReadResult result;
        do
        {
            result = await source.ReadAsync();
            ParseReadResult(source, result);
        }while (!result.IsCompleted);
    }

    private void ParseReadResult(PipeReader source, ReadResult buffer)
    {
        var reader = new SequenceReader<byte>(buffer.Buffer);
        SequencePosition position;
        do
        {
            position = reader.Position;
        } while (ParseReader(ref reader, buffer.IsCompleted));

        source.AdvanceTo(position, buffer.Buffer.End);
    }

    private bool ParseReader(ref SequenceReader<byte> reader, bool bufferIsCompleted)
    {
        if (!NextTokenFinder.SkipToNextToken(ref reader)) return false;
        if (!reader.TryPeek(out var character)) return false;
        return (char)character switch
        {
            '.' or '+' or '-' or (>= '0' and <= '9') => ParseNumber(ref reader, bufferIsCompleted),
            _ => ParseOperator(ref reader, bufferIsCompleted)
        };
    }

    private bool ParseNumber(ref SequenceReader<byte> reader, bool bufferIsCompleted)
    {
        var parser = new NumberWtihFractionParser();
        if (!parser.InnerTryParse(ref reader, bufferIsCompleted)) return false;
        target.HandleNumber(parser.DoubleValue(), parser.IntegerValue());
        return true;
    }

    private bool ParseOperator(ref SequenceReader<byte> reader, bool bufferIsCompleted)
    {
        uint opCode = 0;
        while (true)
        {
            if (!reader.TryPeek(out var character))
            {
                if (bufferIsCompleted && opCode != 0) HandleOpCode(opCode);
                return bufferIsCompleted;
            }
            if (CharClassifier.Classify(character) != CharacterClass.Regular)
            {
                HandleOpCode(opCode);
                return true;
            }
            reader.Advance(1);
            opCode <<= 8;
            opCode |= character;
        }
    }

    private void HandleOpCode(uint opCode) => target.HandleOpCode((ContentStreamOperatorValue)opCode);
}