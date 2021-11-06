using System.Buffers;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.StringParsing;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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
            _ => ValueTask.FromResult(ParseOperator(bfp.WithStartingPosition(reader.Position)))
        };
    }

    private bool ParseSyntaxString(BufferFromPipe bfp)
    {
        var reader = bfp.CreateReader();
        return bfp.LogSuccess(
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
        return ValueTask.FromResult(bpc.LogSuccess(
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

    private bool ParseOperator(in BufferFromPipe bfp)
    {
        var reader = bfp.CreateReader();
        uint opCode = 0;
        while (true)
        {
            if (!reader.TryPeek(out var character))
            {
                if (bfp.Done && opCode != 0) HandleOpCode(opCode);
                return bfp.LogSuccess(bfp.Done, ref reader);
            }
            if (CharClassifier.Classify(character) != CharacterClass.Regular)
            {
                HandleOpCode(opCode);
                bfp.Consume(reader.Position);
                return true;
            }
            reader.Advance(1);
            opCode <<= 8;
            opCode |= character;
        }
    }

    private void HandleOpCode(uint opCode) => target.HandleOpCode((ContentStreamOperatorValue)opCode);
    
    private ValueTask<bool> TryParseDictionary(in BufferFromPipe bfp)
    {
        var reader = bfp.CreateReader();
        var skipper = new DictionarySkipper(ref reader);
        if (!skipper.TrySkipDictionary()) return ValueTask.FromResult(false);
        var clippedseq = reader.UnreadSequence.Slice(0, skipper.CurrentPosition);
        target.HandleString(clippedseq.ToArray());
        reader.Advance(clippedseq.Length);
        bfp.Consume(reader.Position);
        return ValueTask.FromResult(true);
    }

}