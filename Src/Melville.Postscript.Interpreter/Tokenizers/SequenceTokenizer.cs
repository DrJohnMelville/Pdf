using System;
using Melville.Postscript.Interpreter.Values;
using System.Buffers;
using System.Diagnostics;
using System.Net.Http.Headers;
using Melville.Parsing.SequenceReaders;
using Melville.Postscript.Interpreter.Values.Interfaces;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Postscript.Interpreter.Tokenizers;

internal static class SequenceTokenizer
{
    public static bool TryGetPostscriptToken(
        this ref SequenceReader<byte> reader, out PostscriptValue value)
    {
        if (!reader.TryPeekNextNonComment(out var firstChar))
            return default(PostscriptValue).AsFalseValue(out value);
        return firstChar switch
        {
            (byte)'/' => new NameTokenizer(StringKind.LiteralName)
                .TryParse(ref reader.WithAdvance(), out value),
            (byte)'[' or (byte)']' or (byte)'{' or (byte)'}' =>
                TryCopyLiteralName(1, ref reader, out value),
            (byte)'<' => TryParseOpenWakka(ref reader.WithAdvance(), out value),
            (byte)'>' => TryParseCloseWakka(ref reader.WithAdvance(), out value),
            (byte)'(' => new StringTokenizer<SyntaxStringDecoder, int>()
                            .Parse(ref reader.WithAdvance(), out value),
            _ => new NameTokenizer(StringKind.Name).TryParse(ref reader, out value)
        };
    }

   private static bool TryCopyLiteralName(
        int length, ref SequenceReader<byte> reader, out PostscriptValue value)
    {
        Span<byte> name = stackalloc byte[length];
        reader.TryCopyTo(name);
        reader.Advance(length);
        value = PostscriptValueFactory.CreateString(name, StringKind.Name);
        return true;
    }

    private static bool TryParseOpenWakka
        (ref SequenceReader<byte> reader, out PostscriptValue value)
    {
        if (!reader.TryRead(out var secondChar)) 
            return default(PostscriptValue).AsFalseValue(out value);
        return secondChar switch
        {
            (byte)'<' => PostscriptValueFactory.CreateString("<<"u8, StringKind.Name)
                .AsTrueValue(out value),
            (byte)'~'=> DecodeAscii85String(ref reader, out value),
            _ => new StringTokenizer<HexStringDecoder, byte>().Parse(ref reader.WithRewind(), out value),
        };

    }

    private static bool DecodeAscii85String(ref SequenceReader<byte> reader, out PostscriptValue value) => 
        new StringTokenizer<Ascii85StringDecoder, byte>().Parse(ref reader, out value) && reader.TryAdvance(1);

    private static bool TryParseCloseWakka(
        ref SequenceReader<byte> reader, out PostscriptValue value)
    {
        if (!reader.TryRead(out var secondChar))
            return default(PostscriptValue).AsFalseValue(out value);
        return secondChar is (byte)'>'
            ? PostscriptValueFactory.CreateString(">>"u8, StringKind.Name)
                .AsTrueValue(out value)
            : throw new PostscriptNamedErrorException(">> expected.", "syntaxerror");
    }


}