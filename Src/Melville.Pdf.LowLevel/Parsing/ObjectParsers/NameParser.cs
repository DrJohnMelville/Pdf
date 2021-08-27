using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    public class NameParser: PdfAtomParser
    {
        public override bool TryParse(ref SequenceReader<byte> bytes, bool final,[NotNullWhen(true)] out PdfObject? output)
        {
            output = null;
            if (!TrySkipSolidus(ref bytes)) return final;
            var copyOfBytes = bytes;
            if (!TryComputeLengthAndHash(ref bytes, final, out var length, out var hash)) return false;
            if (LookupNameByHash(ref copyOfBytes, hash, length, out output)) 
                return true;
            CreateNovelName(ref copyOfBytes, length, out output);
            return true;
        }

        private static bool TrySkipSolidus(ref SequenceReader<byte> bytes)
        {
            if (bytes.Remaining < 1) return false;
            bytes.Advance(1);
            return true;
        }

        private static bool TryComputeLengthAndHash(ref SequenceReader<byte> bytes, bool final, out int length,
            out uint hash)
        {
            length = 0;
            hash = FnvHash.EmptyStringHash();
            while (true)
            {
                switch (GetNextNameCharacter(ref bytes, out var character))
                {
                    case NextCharResult.NotEnoughChars:
                        return final;
                    case NextCharResult.Terminating:
                        bytes.Rewind(1);
                        return true;
                }
                length++;
                hash = FnvHash.SingleHashStep(hash, character);
            } 
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
            ref SequenceReader<byte> bytes, ref byte character)
        {
            if (!(bytes.TryRead(out var mostSig) && bytes.TryRead(out var leastSig)))
            {
                return NextCharResult.NotEnoughChars;
            }
            character = HexMath.ByteFromHexCharPair(mostSig, leastSig);
            return NextCharResult.NonTerminatingChar;
        }

        private static NextCharResult DoesCharTerminateName(byte character) =>
            CharClassifier.Classify(character) == CharacterClass.Regular?
                NextCharResult.NonTerminatingChar: NextCharResult.Terminating;

        private static void CreateNovelName(ref SequenceReader<byte> bytes, int length, out PdfObject output)
        {
            var buffer = new byte[length];
            for (int i = 0; i < length; i++)
            {
                var ret = GetNextNameCharacter(ref bytes, out buffer[i]);
                Debug.Assert(ret == NextCharResult.NonTerminatingChar);
            }

            output = new PdfName(buffer);
        }

        private static bool LookupNameByHash(
            ref SequenceReader<byte> bits, uint hash, int length, [NotNullWhen(true)] out PdfObject? foundName)
        {
            var savedBits = bits; // need a save point if we fail.
            if (TryFindKnownName(ref bits, hash, length, out var lookupResult))
            {
                foundName = lookupResult.PreferredName();
                return true;
            }
            foundName = null;
            bits = savedBits; // return to the beginning for a third pass to read the name
            return false;
        }

        private static bool TryFindKnownName(
            ref SequenceReader<byte> bits, uint hash, int length, [NotNullWhen(true)]out PdfName? lookupResult) =>
            KnownNames.LookupName(hash, out lookupResult) && 
            CandidateHasCorrectLength(length, lookupResult) &&
            VerifyEquality(ref bits, lookupResult.Bytes);

        private static bool CandidateHasCorrectLength(int length, PdfName lookupResult) => 
            lookupResult.Bytes.Length == length;

        // we guarentee that the known names do not collide with each other, but arbitrary names
        // in a PDF file could collide with the known names, so when we get a hit on the dictionary
        // we have to check against the source to make sure that we actually match.
        private static bool VerifyEquality(ref SequenceReader<byte> bits, byte[] pattern)
        {
            foreach (var character in pattern)
            {
                if (!(GetNextNameCharacter(ref bits, out var comp) == NextCharResult.NonTerminatingChar
                      && comp == character)) return false;
            }
            return true;
        }
    }
}