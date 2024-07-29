using System.Buffers;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.MultiplexSources;
using Melville.Parsing.SequenceReaders;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal readonly struct CffEncodingReader(IByteSource source)
{
    private readonly byte[] result = new byte[256];
    public async ValueTask<byte[]> ParseAsync()
    {
        var type = await source.ReadBigEndianUintAsync(1).CA();
        switch (type & 0x7F)
        {
            case 0:
                await ParseType0Async().CA();
                break;
            case 1:
                await ParseType1Async().CA();
                break;
            default:
                throw new InvalidDataException("Invalid encoding type");
        }
        source.Dispose();
        return result;
    }

    private async ValueTask ParseType0Async()
    {
        var count = (byte)await source.ReadBigEndianUintAsync(1).CA();
        var buffer = await source.ReadAtLeastAsync(count).CA();
        ParseType0(buffer.Buffer, count);
        source.AdvanceTo(buffer.Buffer.GetPosition(count));
    }

    private void ParseType0(ReadOnlySequence<byte> bufferBuffer, byte count)
    {
        var reader = new SequenceReader<byte>(bufferBuffer);
        for (byte i = 1; i <= count; i++)
        {
            result[reader.ReadBigEndianUint8()] = i;
        }
    }

    
    private async ValueTask ParseType1Async()
    {
        var count = (byte)await source.ReadBigEndianUintAsync(1).CA();
        var length = count * 2;
        var buffer = await source.ReadAtLeastAsync(length).CA();
        ParseType1(buffer.Buffer, count);
        source.AdvanceTo(buffer.Buffer.GetPosition(length));
    }

    private void ParseType1(ReadOnlySequence<byte> buffer, byte count)
    {
        var source = new SequenceReader<byte>(buffer);
        byte glyph = 1;
        for (int i = 0; i < count; i++)
        {
            var first = source.ReadBigEndianUint8();
            var innerCount = source.ReadBigEndianUint8();
            for (int j = 0; j < innerCount; j++)
            {
                result[first + j] = glyph++;
            }
        }
    }
}