using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs
{
    internal class CharSetReader(
        CffStringIndex strings,
        IByteSource charsetPipe,
        long numGlyphs)
    {
        private readonly string[] finalBuffer = new string[numGlyphs];

        public async ValueTask<string[]> ReadCharSetAsync()
        {
            finalBuffer[0] = ".notdef";
            var type = await charsetPipe.ReadBigEndianUintAsync(1).CA();
            switch (type)
            {
                case 0:
                    await ParseType0SetAsync().CA();
                    break;
                case 1:
                    await ParseType1or2SetAsync(1).CA();
                    break;
                case 2:
                    await ParseType1or2SetAsync(2).CA();
                    break;
                default:
                    throw new InvalidDataException("Invalid charset type");
            }

            return finalBuffer;
        }

        private async ValueTask ParseType0SetAsync()
        {
            for (int i = 1; i < finalBuffer.Length; i++)
            {
                finalBuffer[i] = await strings.GetNameAsync(
                    (int)await charsetPipe.ReadBigEndianUintAsync(2).CA()).CA();
            }
        }

        private async ValueTask ParseType1or2SetAsync(int nLength)
        {
            int position = 1;
            while (position < finalBuffer.Length)
            {
                var first = (int)await charsetPipe.ReadBigEndianUintAsync(2).CA();
                var count = (int)await charsetPipe.ReadBigEndianUintAsync(nLength).CA();
                for (int i = 0; i < count + 1; i++)
                {
                    finalBuffer[position++] = await strings.GetNameAsync(first + i).CA();
                }
            }
        }
    }
}