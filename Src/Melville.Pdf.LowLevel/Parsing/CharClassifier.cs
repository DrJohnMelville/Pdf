using System.Buffers;

namespace Melville.Pdf.LowLevel.Parsing
{
    public enum CharacterClass
    {
        Regular = 0,
        White = 1,
        Delimiter = 2
    }

    public static class CharClassifier
    {
        public static CharacterClass Classify(byte input)
        {
            return input switch
            {
                0 or 0x9 or 0xA or 0xC or 0xD or 0x20
                    => CharacterClass.White,
                (byte)'(' or (byte)')' or (byte)'<' or (byte)'>' or (byte)'[' or (byte)']' or
                    (byte)'{' or (byte)'}' or (byte)'/' or (byte)'%' 
                    => CharacterClass.Delimiter,
                _ => CharacterClass.Regular
            };
        }
        
        private static readonly byte[] EndOfLineMarkers = {10, 13};
        public static bool TrySkipToEndOfLineMarker(this ref SequenceReader<byte> reader) => 
            reader.TryAdvanceToAny(EndOfLineMarkers, false);
    }
}