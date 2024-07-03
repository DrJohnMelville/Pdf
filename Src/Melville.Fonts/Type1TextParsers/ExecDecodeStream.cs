using System.Buffers;
using System.Security.Cryptography.X509Certificates;
using Melville.Hacks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams.Bases;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Fonts.Type1TextParsers;

internal class ExecDecodeStream(Stream source, ushort pkey) 
    : DefaultBaseStream(true, false, true)
{
    private ushort key = pkey;
    private bool notInitialized = true;
    public override int Read(Span<byte> buffer)
    {
        if (notInitialized) Initalize(buffer);
        var ret = source.Read(buffer);
        DecodeType1Encoding.DecodeSegment(buffer[..ret], ref key);
        return ret;
    }

    private void Initalize(Span<byte> buffer)
    {
        notInitialized = false;
        var ret = source.Read(buffer[..1]);
        int remainder = 0;
        if (CharacterClassifier.IsWhitespace(buffer[0]))
        {
            remainder = 4;
        }
        else
        {
            DecodeType1Encoding.DecodeSegment(buffer[..1], ref key);
            remainder = 3;
        }

        for (int i = 0; i < remainder; i++)
        {
            source.Read(buffer[..1]);
            DecodeType1Encoding.DecodeSegment(buffer[..1], ref key);
        }
    }

    private async ValueTask InitalizeAsync(Memory<byte> buffer)
    {
        notInitialized = false;
        var ret = await source.ReadAsync(buffer[..1]).CA();
        int remainder = 0;
        if (CharacterClassifier.IsWhitespace(buffer.Span[0]))
        {
            remainder = 4;
        }
        else
        {
            DecodeType1Encoding.DecodeSegment(buffer.Span[..1], ref key);
            remainder = 3;
        }

        for (int i = 0; i < remainder; i++)
        {
            await source.ReadAsync(buffer[..1]).CA();
            DecodeType1Encoding.DecodeSegment(buffer.Span[..1], ref key);
        }
    }

    public override async ValueTask<int> ReadAsync(
        Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (notInitialized) await InitalizeAsync(buffer).CA();
        var ret = await source.ReadAsync(buffer, cancellationToken).CA();
        DecodeType1Encoding.DecodeSegment(buffer.Span[..ret], ref key);
        return ret;
    }
}

internal static class DecodeType1Encoding
{
    //Type 1 file format spec page 63
    private static ushort c1 = 52845;
    private static ushort c2 = 22719;
    public static void DecodeSegment(Span<byte> buffer, ref ushort key)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            var cipher = buffer[i];
            var plain = (byte)(cipher ^ (key >> 8));
            key = (ushort)((cipher + key) * c1 + c2);
            buffer[i] = plain;
        }
    }

    public static Span<byte> DecodeSpan(Span<byte> input, ushort key, int leadBytes = 4)
    {
        DecodeSegment(input, ref key);
        return input[leadBytes..];
    }
}