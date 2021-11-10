using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamDataSources;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Primitives.PipeReaderWithPositions;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Pdf.LowLevel.Parsing.StringParsing;
using Melville.Pdf.LowLevel.Writers;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

public readonly struct ContentStreamParser
{
    private readonly ContentStreamContext target;

    public ContentStreamParser(IContentStreamOperations target)
    {
        this.target = new ContentStreamContext(target);
    }

    public async ValueTask Parse(PipeReader source)
    {
        bool done = false;
        do
        {
            var bfp = await BufferFromPipe.Create(source);
            done = (! await ParseReadResult(bfp)) && bfp.Done;
            
        }while (!done);
    }

    private async ValueTask<bool> ParseReadResult(BufferFromPipe bfp)
    {
        if (await ParseReader(bfp)) return true;
        bfp.NeedMoreBytes();
        return false;
    }

    
    private ValueTask<bool> ParseReader(in BufferFromPipe bfp)
    {
        var reader = bfp.CreateReader();
        if (!SkipWhiteSpaceAndBrackets(ref reader)) return ValueTask.FromResult(false);
        if (!reader.TryPeek(out var character)) return ValueTask.FromResult(false);
        return (char)character switch
        {
            '.' or '+' or '-' or (>= '0' and <= '9') => 
                ValueTask.FromResult(ParseNumber(bfp.WithStartingPosition(reader.Position))),
            '/' => ValueTask.FromResult(ParseName(bfp.WithStartingPosition(reader.Position))),
            '(' => ValueTask.FromResult(ParseSyntaxString(bfp.WithStartingPosition(reader.Position))),
            '<' => HandleInitialOpenWakka(bfp.WithStartingPosition(reader.Position)),
            _ => ParseOperator(bfp.WithStartingPosition(reader.Position))
        };
    }

    private bool ParseSyntaxString(BufferFromPipe bfp)
    {
        var reader = bfp.CreateReader();
        return bfp.ConsumeIfSucceeded(
            HandleParsedString(SyntaxStringParser.TryParseToBytes(ref reader, bfp.Done)),
            ref reader);
    }

    private ValueTask<bool> HandleInitialOpenWakka(in BufferFromPipe bpc)
    {
        var reader = bpc.CreateReader();
        if (!reader.TryPeek(1, out var b2)) return ValueTask.FromResult(false);
        return b2 == (byte)'<'?
            TryParseDictionary(bpc): 
            ParseHexString(bpc);
    }

    private ValueTask<bool> ParseHexString(BufferFromPipe bpc)
    {
        var reader = bpc.CreateReader();
        return ValueTask.FromResult(bpc.ConsumeIfSucceeded(
            HandleParsedString(HexStringParser.TryParseToBytes(ref reader, bpc.Done)),
            ref reader));
    }

    private bool HandleParsedString(byte[]? str)
    {
        if (str == null) return false;
        target.HandleString(str);
        return true;
    }

    private bool ParseName(in BufferFromPipe bfp )
    {
        var reader = bfp.CreateReader();
        if (!NameParser.TryParse(ref reader, bfp.Done, out var name)) 
            return false;
        bfp.Consume(reader.Position);
        target.HandleName(name);
        return true;
    }

    private static bool SkipWhiteSpaceAndBrackets(ref SequenceReader<byte> reader)
    {
        while (true)
        {
            if (!reader.TryPeek(out var peeked)) return false;
            if (CharClassifier.Classify(peeked) != CharacterClass.White &&
                (char)peeked is not ('[' or ']')) return true;
            reader.Advance(1);
        }
    }

    private bool ParseNumber(in BufferFromPipe bfp)
    {
        var reader = bfp.CreateReader();
        var parser = new NumberWtihFractionParser();
        if (!parser.InnerTryParse(ref reader, bfp.Done)) return false;
        bfp.Consume(reader.Position);
        target.HandleNumber(parser.DoubleValue(), parser.IntegerValue());
        return true;
    }

    private ValueTask<bool> ParseOperator(BufferFromPipe bfp)
    {
        var reader = bfp.CreateReader();
        uint opCode = 0;
        while (true)
        {
            switch (reader.TryPeek(out var character), CharClassifier.Classify(character), bfp.Done, opCode)
            {
                case (true, _, _, (int)ContentStreamOperatorValue.BI):
                    return ParseInlineImage(bfp);
                case (false,_, true, not 0):
                case (true, not CharacterClass.Regular, _, _):    
                    bfp.Consume(reader.Position);
                    return RunOpcode(opCode);
                case (false, _, _, _):
                    bfp.NeedMoreBytes();
                    return ValueTask.FromResult(false);
            }
            reader.Advance(1);
            opCode <<= 8;
            opCode |= character;
        }
    }
    
    private async ValueTask<bool> RunOpcode(uint opCode)
    {
        await HandleOpCode(opCode);
        return true;
    }

    private ValueTask HandleOpCode(uint opCode) => target.HandleOpCode((ContentStreamOperatorValue)opCode);
    
    private async ValueTask<bool> TryParseDictionary(BufferFromPipe bfp)
    {
        var dict = await PdfParserParts.EmbeddedDictionaryParser.ParseAsync(bfp.CreateParsingReader());
        target.HandleDictionary(dict);
        return true;
    }
    private async ValueTask<bool> ParseInlineImage(BufferFromPipe bfp)
    {
        var dict = await PdfParserParts.InlineImageDictionaryParser
            .ParseDictionaryItemsAsync(bfp.CreateParsingReader());
        bfp = await bfp.Refresh();
        SequencePosition endPos;
        while (!SearchForEndSequence(bfp, out endPos))
        {
            bfp = await bfp.InvalidateAndRefresh();
        }

        await target.HandleInlineImage(CreateStream(GrabStreamContent(bfp, endPos), dict));
        return true;
    }
    
    private static PdfStream CreateStream(byte[] data, Dictionary<PdfName, PdfObject> dict)
    {
        return new PdfStream(
            new LiteralStreamSource(data, StreamFormat.DiskRepresentation), dict);
    }

    private byte[] GrabStreamContent(BufferFromPipe bfp, SequencePosition endPos)
    {
        var buffer = TrimInitialWhiteSpaceAndTerminalOperator(bfp, endPos);
        var data = CopeSequenceToBuffer(buffer);
        bfp.Consume(endPos);
        return data;
    }

    private static ReadOnlySequence<byte> TrimInitialWhiteSpaceAndTerminalOperator(
        in BufferFromPipe bfp, SequencePosition endPos) => bfp.Buffer.Slice(
        1, bfp.Buffer.Length-3);

    private byte[] CopeSequenceToBuffer(ReadOnlySequence<byte> buffer)
    {
        var data = new byte[buffer.Length];
        buffer.CopyTo(data);
        return data;
    }
    

    private bool SearchForEndSequence(in BufferFromPipe bfp,  out SequencePosition endPos)
    {
        int position = 0;
        var seqReader = bfp.CreateReader();
        while (seqReader.TryRead(out var current))
        {
            switch ((char) current, position)
            {
                case ('E', 0):
                    position++;
                    break;
                case ('I', 1):
                    endPos = seqReader.Position;
                    return true;
                default:
                    position = 0;
                    break;
            }
        }

        endPos = seqReader.Position;
        return false;
    }
}