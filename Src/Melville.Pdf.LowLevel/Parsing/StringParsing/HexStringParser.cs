using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.StringParsing;

internal class HexStringParser: PdfAtomParser
{
    public override bool TryParse(
        ref SequenceReader<byte> reader, bool final, IParsingReader source,
        [NotNullWhen(true)]out PdfObject? obj)
    {
        reader.Advance(1);
        var copyOfReader = reader;
        if (!TryCount(ref reader, final, out var len))
        {
            obj = null;
            return false;
        }

        var stringBytes = ReadString(ref copyOfReader, len);
        obj = source.CreateDecryptedString(stringBytes);
        return true;
    }

    public static byte[]? TryParseToBytes(ref SequenceReader<byte> reader, bool final)
    {
        reader.Advance(1);
        var copyOfReader = reader;
        return TryCount(ref reader, final, out var len) ? 
            ReadString(ref copyOfReader, len) : 
            null;
    }
        
    private static bool TryCount(ref SequenceReader<byte> reader, bool final, out int length)
    {
        length = 0;
        while (true)
        {
            switch (GetNibble(ref reader))
            {
                case Nibble.OutOfSpace: return final;
                case Nibble.Terminator: return true;
            }
            length++;
            switch (GetNibble(ref reader))
            {
                case Nibble.OutOfSpace: return final;
                case Nibble.Terminator: return true;
            }
        }
    }

    private static byte[] ReadString(
        ref SequenceReader<byte> reader, int length)
    {
        var buff = new byte[length];
        for (int i = 0; i < length; i++)
        {
            buff[i] = HexMath.ByteFromNibbles(GetNibble(ref reader), GetNibble(ref reader));
        }

        return buff;
    }

    private static Nibble GetNibble(ref SequenceReader<byte> reader)
    {
        byte item;
        do
        {
            if (!reader.TryRead(out item)) return Nibble.OutOfSpace;
        } while (CharClassifier.Classify(item) == CharacterClass.White);
        return HexMath.ByteToNibble(item);
    }
}