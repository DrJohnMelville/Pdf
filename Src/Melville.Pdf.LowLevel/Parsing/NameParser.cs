using System;
using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model;

namespace Melville.Pdf.LowLevel.Parsing
{
    public class NameParser
    {
        public static bool TryParse(ref SequenceReader<byte> bytes,[NotNullWhen(true)] out PdfName? output)
        {
            output = null;
            if (!TrySkipSolidus(ref bytes)) return false;
            var copyOfBytes = bytes;
            if (!TryComputeLength(ref bytes, out var length)) return false;
            CreateNovelName(ref copyOfBytes, length, out output);
            return true;
        }

        private static bool TrySkipSolidus(ref SequenceReader<byte> bytes)
        {
            if (bytes.Remaining < 2) return false;
            bytes.Advance(1);
            return true;
        }

        private static bool TryComputeLength(ref SequenceReader<byte> bytes, out int length)
        {
            length = -1;
            byte character;
            do
            {
                if (!GetNextNameCharacter(ref bytes, out character)) return false;
                length++;
            } while (CharClassifier.Classify(character) == CharacterClass.Regular);
            bytes.Rewind(1);
            return true;
        }

        private static bool GetNextNameCharacter(ref SequenceReader<byte> bytes, out byte character)
        {
            return bytes.TryRead(out character);
        }

        private static void CreateNovelName(ref SequenceReader<byte> bytes, int length, out PdfName output)
        {
            var buffer = new byte[length];
            for (int i = 0; i < length; i++)
            {
                var ret = GetNextNameCharacter(ref bytes, out buffer[i]);
                Debug.Assert(ret);
            }

            output = new PdfName(buffer);
        }
    }
}