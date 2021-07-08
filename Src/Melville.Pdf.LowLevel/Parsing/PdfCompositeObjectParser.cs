using System;
using System.Buffers;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model;
using Melville.Pdf.LowLevel.Parsing.NameParsing;
using Melville.Pdf.LowLevel.Parsing.PdfStreamHolders;
using Melville.Pdf.LowLevel.Parsing.StringParsing;

namespace Melville.Pdf.LowLevel.Parsing
{
    public interface IPdfObjectParser
    {
        public Task<PdfObject> ParseAsync(ParsingSource source);

    }


    public class PdfCompositeObjectParser:  IPdfObjectParser
    {
        public async Task<PdfObject> ParseAsync(ParsingSource source)
        {
            await NextTokenFinder.SkipToNextToken(source);
            while (true)
            {
                var data = await source.ReadAsync();
                var parser = PickParser(data.Buffer);
                if (parser != null)
                {
                    return await parser.ParseAsync(source);
                }
                source.AdvanceTo(data.Buffer.Start, data.Buffer.End);
            }
        }

        private IPdfObjectParser? PickParser(ReadOnlySequence<byte> dataBuffer)
        {
            var reader = new SequenceReader<byte>(dataBuffer);
            if (!(reader.TryRead(out var firstByte) && reader.TryRead(out var secondByte)))
            {
                return null;
            }
            return (firstByte, secondByte) switch
            {
                ((byte) '<', _) => HexString,
                ((byte) '(', _) => SyntaxString,
                ((byte) '[', _) => PdfArray,
                ( >= (byte) '0' and <= (byte)'9', _) => Number,
                ((byte) '+' or (byte)'-', _) => Number,
                ((byte) '/', _) => Names,
                ((byte) 't', _) => TrueParser,
                ((byte) 'f', _) => FalseParser,
                ((byte) ']', _) => ArrayTermination,
                ((byte) '>', (byte)'>') => DictionatryTermination,
                _ => NullParser
            };
        }

        /*
        public override bool TryParse(ref SequenceReader<byte> reader, out PdfObject? obj)
        {
            #warning this is temporary and broken to assume PdfAtomParser
            if (reader.TryPeek(out byte first))
                return ((PdfAtomParser)catalog[first]).TryParse(ref reader, out obj);
                    
            obj = null;
            return false;
        }
    */

        private static readonly HexStringParser HexString = new();
        private static readonly SyntaxStringParser SyntaxString = new();
        private static readonly PdfArrayParser PdfArray = new();
        private static readonly NumberParser Number = new();
        private static readonly NameParser Names = new();
        private static LiteralTokenParser TrueParser = new(4, PdfBoolean.True);
        private static LiteralTokenParser FalseParser = new(5, PdfBoolean.False);
        private static LiteralTokenParser NullParser = new(4, PdfNull.Instance);
        private static LiteralTokenParser ArrayTermination = new(1, PdfNull.ArrayTerminator);
        private static LiteralTokenParser DictionatryTermination = new(2, PdfNull.ArrayTerminator);
    }
}