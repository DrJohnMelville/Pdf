using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal class NameParser: PdfAtomParser
{
    public override bool TryParse(
        ref SequenceReader<byte> bytes, bool final, IParsingReader source,
        [NotNullWhen(true)] out PdfObject? output)
    {
        var ret = TryParse(ref bytes, final, out var name);
        output = name;
        return ret;
    }

    public static bool TryParse(
        ref SequenceReader<byte> bytes, bool final, [NotNullWhen(true)]out PdfName? output)
    {
        output = null;
        if (!TrySkipSolidus(ref bytes)) return final;
        output = GetNameText(ref bytes, final);
        return output is not null;
    }

    private static PdfName? GetNameText(ref SequenceReader<byte> bytes, bool final)
    {
        var length = 0;
        Span<byte> name = stackalloc byte[128]; // longest possible name per Annex C + 1
        while (true)
        {
            switch (GetNextNameCharacter(ref bytes, out name[length]))
            {
                case NextCharResult.NotEnoughChars:
                    return final ? LookupName(name, length): null;
                case NextCharResult.Terminating:
                    bytes.Rewind(1);
                    return LookupName(name, length);
            }
            length++;
        } 
    }

    private static PdfName LookupName(in Span<byte> name, int length) => 
        NameDirectory.Get(name[..length]);

    private static bool TrySkipSolidus(ref SequenceReader<byte> bytes)
    {
        if (!bytes.TryRead(out var solius)) return false;
        if (solius != (byte)'/')
            throw new PdfParseException("Names must start with a '/'.");
        return true;
    }
        
    private enum NextCharResult
    {
        NotEnoughChars,
        NonTerminatingChar,
        Terminating
    }
    private static NextCharResult GetNextNameCharacter(ref SequenceReader<byte> bytes, out byte character)
    {
        if (!bytes.TryRead(out character)) return NextCharResult.NotEnoughChars;
            
        return character != '#' ? 
            DoesCharTerminateName(character) : 
            ParseTwoCharacterHexSequence(ref bytes, ref character);
    }

    private static NextCharResult ParseTwoCharacterHexSequence(
        scoped ref SequenceReader<byte> bytes, scoped ref byte character)
    {
        if (!(bytes.TryRead(out var mostSig) && bytes.TryRead(out var leastSig)))
        {
            return NextCharResult.NotEnoughChars;
        }
        character = HexMath.ByteFromHexCharPair(mostSig, leastSig);
        return NextCharResult.NonTerminatingChar;
    }

    private static NextCharResult DoesCharTerminateName(byte character) =>
        CharClassifier.IsRegular(character)?
            NextCharResult.NonTerminatingChar: NextCharResult.Terminating;
}