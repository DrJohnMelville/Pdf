using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Parsing.StringParsing
{
    public class HexStringParser: PdfAtomParser
    {
        public override bool TryParse(
            ref SequenceReader<byte> reader, [NotNullWhen(true)]out PdfObject? obj)
        {
            reader.Advance(1);
            var copyOfReader = reader;
            if (!TryCount(ref reader, out var len))
            {
                obj = null;
                return false;
            }

            obj = ReadString(ref copyOfReader, len);
            return true;
        }

        private bool TryCount(ref SequenceReader<byte> reader, out int length)
        {
            length = 0;
            while (true)
            {
                switch (GetNibble(ref reader))
                {
                    case Nibble.OutOfSpace: return false;
                    case Nibble.Terminator: return true;
                }
                length++;
                switch (GetNibble(ref reader))
                {
                    case Nibble.OutOfSpace: return false;
                    case Nibble.Terminator: return true;
                }
            }
        }

        private PdfObject ReadString(ref SequenceReader<byte> reader, int length)
        {
            var buff = new byte[length];
            for (int i = 0; i < length; i++)
            {
                buff[i] = HexMath.ByteFromNibbles(GetNibble(ref reader), GetNibble(ref reader));
            }
            return new PdfString(buff);
        }

        private Nibble GetNibble(ref SequenceReader<byte> reader)
        {
            byte item;
            do
            {
                if (!reader.TryRead(out item)) return Nibble.OutOfSpace;
            } while (CharClassifier.Classify(item) == CharacterClass.White);
            return HexMath.ByteToNibble(item);
        }
    }
}