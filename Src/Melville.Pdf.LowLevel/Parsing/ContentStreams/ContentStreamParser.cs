using System.Buffers;
using System.IO.Pipelines;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.ContentStreams;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ContentStreams.EmbeddedImageParsing;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Parsing.ContentStreams;

/// <summary>
/// Parses a content stream (expressed as a PipeReader) and "renders" it to an IContentStreamOperations.
/// </summary>
public readonly struct ContentStreamParser
{
    private readonly ContentStreamContext target;

    /// <summary>
    /// Create the ContentStreamParser
    /// </summary>
    /// <param name="target">The target we wish to render the stream to.</param>
    public ContentStreamParser(IContentStreamOperations target)
    {
        this.target = new ContentStreamContext(target);
    }

    /// <summary>
    /// Render the content stream operations in the given source pipereader.
    /// </summary>
    /// <param name="source">The content stream to parse.</param>
    /// <returns>A valuetask representing this operation.</returns>
    public async ValueTask ParseAsync(PipeReader source)
    {
        bool done;
        do
        {
            var bfp = await BufferFromPipe.CreateAsync(source).CA();
            done = (! await ParseReadResultAsync(bfp).CA()) && bfp.Done;
            
        }while (!done);
    }

    private async ValueTask<bool> ParseReadResultAsync(BufferFromPipe bfp)
    {
        if (await ParseReaderAsync(bfp).CA()) return true;
        bfp.NeedMoreBytes();
        return false;
    }

    
    private ValueTask<bool> ParseReaderAsync(in BufferFromPipe bfp)
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
            '<' => HandleInitialOpenWakkaAsync(bfp.WithStartingPosition(reader.Position)),
            '%' => SkipCommentAsync(bfp.WithStartingPosition(reader.Position)),
            _ => ParseOperatorAsync(bfp.WithStartingPosition(reader.Position))
        };
    }

    private ValueTask<bool> SkipCommentAsync(BufferFromPipe bfp)
    {
        var reader = bfp.CreateReader();
        return new (bfp.ConsumeIfSucceeded(reader.TryAdvanceToAny(endOfLineMarkers, true), ref reader));
    }
    private static readonly byte[] endOfLineMarkers =new byte[] { (byte)'\r', (byte)'\n' }; 

    private bool ParseSyntaxString(BufferFromPipe bfp)
    {
        var reader = bfp.CreateReader();
        return bfp.ConsumeIfSucceeded(
            HandleParsedString(ReadString<SyntaxStringDecoder, int>(ref reader, bfp.Done)),
            ref reader);
    }

    private ValueTask<bool> HandleInitialOpenWakkaAsync(in BufferFromPipe bpc)
    {
        var reader = bpc.CreateReader();
        if (!reader.TryPeek(1, out var b2)) return ValueTask.FromResult(false);
        return b2 == (byte)'<'?
            TryParseDictionaryAsync(bpc): 
            ParseHexStringAsync(bpc);
    }

    private ValueTask<bool> ParseHexStringAsync(BufferFromPipe bpc)
    {
        var reader = bpc.CreateReader();
        return ValueTask.FromResult(bpc.ConsumeIfSucceeded(
            HandleParsedString(ReadString<HexStringDecoder, byte>(ref reader, bpc.Done)),
            ref reader));
    }

    private static byte[]? ReadString<T, TState>(ref SequenceReader<byte> reader, bool final)
        where T: IStringDecoder<TState>, new() where TState: new()
    {
        reader.Advance(1);
        new StringTokenizer<T, TState>().Parse(ref reader, out byte[]? ret);
        return ret;
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
            if (!CharClassifier.IsWhite(peeked) && peeked is not ((byte)'[' or (byte)']')) return true;
            reader.Advance(1);
        }
    }

    private bool ParseNumber(in BufferFromPipe bfp)
    {
        var reader = bfp.CreateReader();
        var parser = new NumberWithFractionParser();
        if (!parser.InnerTryParse(ref reader, bfp.Done)) return false;
        bfp.Consume(reader.Position);
        target.HandleNumber(parser.DoubleValue(), parser.IntegerValue());
        return true;
    }

    private ValueTask<bool> ParseOperatorAsync(BufferFromPipe bfp)
    {
        var reader = bfp.CreateReader();
        uint opCode = 0;
        while (true)
        {
            switch (reader.TryPeek(out var character), CharClassifier.Classify(character), bfp.Done, opCode)
            {
                case (true, _, _, (int)ContentStreamOperatorValue.BI):
                    return InlineImageParser.ParseInlineImageAsync(bfp, target);
                case (false,_, true, not 0):
                case (true, not CharacterClass.Regular, _, _):    
                    bfp.Consume(reader.Position);
                    return RunOpcodeAsync(opCode);
                case (false, _, _, _):
                    bfp.NeedMoreBytes();
                    return ValueTask.FromResult(false);
            }
            reader.Advance(1);
            opCode <<= 8;
            opCode |= character;
        }
    }
    
    private async ValueTask<bool> RunOpcodeAsync(uint opCode)
    {
        await HandleOpCodeAsync(opCode).CA();
        return true;
    }

    private ValueTask HandleOpCodeAsync(uint opCode) => target.HandleOpCodeAsync((ContentStreamOperatorValue)opCode);
    
    private async ValueTask<bool> TryParseDictionaryAsync(BufferFromPipe bfp)
    {
        var dict = await PdfParserParts.EmbeddedDictionaryParser.ParseAsync(bfp.CreateParsingReader()).CA();
        target.HandleDictionary(dict);
        return true;
    }
}