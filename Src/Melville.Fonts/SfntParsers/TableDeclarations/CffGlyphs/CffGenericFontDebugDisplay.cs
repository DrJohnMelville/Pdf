﻿using System.Buffers;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Parsing.ParserMapping;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Melville.Fonts.SfntParsers.TableDeclarations.CffGlyphs;

internal partial class CffGenericFont
{
#if DEBUG
    public ValueTask TryAddToParseMapAsync()
    {
        if (!topDictData.IsLoggingParseMap()) return ValueTask.CompletedTask;
        return AddToParseMapAsync();
    }

    private async ValueTask AddToParseMapAsync()
    {
        await ParsePrivateDictAsync().CA();
        await ParseCharSetAsync().CA();
        await ParseEncodingAsync().CA();
    }

    private async Task ParsePrivateDictAsync()
    {
        var bookmark = topDictData.BookmarkAt(topDictData.PrivateOffset);
        if (topDictData.PrivateOffset is 0)
        {
            bookmark.LogParsePosition("No Private Dictionary");
            return;
        }

        bookmark.IndentParseMap("Private Dict");
        bookmark.JumpToParseMap(0);

        await topDictData.HandlePrivateDictBytesAsync(pb =>
        {
            var dictParser = new DictParser<CffDictionaryDefinition>(
                new SequenceReader<byte>(pb),
                bookmark, stackalloc DictValue[1]);
            while (dictParser.ReadNextInstruction(0) is not 0xFF)
            {
                // do nothing
            }

            return 0;
        }).CA();
        bookmark.OutdentParseMap();
    }

    private ValueTask ParseCharSetAsync()
    {
        var bookMark = topDictData.BookmarkAt(topDictData.CharsetOffset);
        return topDictData.CharsetOffset switch
        {
            0 => LogBuiltinAsync("Built-in CharSet IsoAdobe", bookMark),
            1 => LogBuiltinAsync("Built-in CharSet Expert", bookMark),
            2 => LogBuiltinAsync("Built-in CharSet ExpertSubset", bookMark),
            _ => CustomCharsetAsync(bookMark)
        };
    }

    private ValueTask LogBuiltinAsync(string charset, ParseMapBookmark? bookmark)
    {
        bookmark.LogParsePosition(charset);
        return ValueTask.CompletedTask;
    }

    private async ValueTask CustomCharsetAsync(ParseMapBookmark? bookMark)
    {
        bookMark.IndentParseMap("Custom Char Map");
        bookMark.JumpToParseMap(0);
        using var charsetPipe = topDictData.CharsetPipe();
        bookMark.AddParseMapAlias(charsetPipe);
        await new CharSetReader<FakeCharsetTarget>(charsetPipe, new FakeCharsetTarget(charsetPipe, 
            charStringIndex.Length)).ReadCharSetAsync().CA();
        bookMark.OutdentParseMap();
    }

    internal readonly struct FakeCharsetTarget(IByteSource charsetPipe, uint length) : ICharSetTarget
    {
        public long Count => length;
        public ValueTask SetGlyphNameAsync(int index, ushort SID)
        {
            charsetPipe.LogParsePosition($"{SID} (0x{SID:X}) => {index} (0x{index:X})");
            return ValueTask.CompletedTask;
        }
    }

    private ValueTask ParseEncodingAsync()
    {
        var bookmark = topDictData.BookmarkAt((int)topDictData.EncodingOffset);
        bookmark.JumpToParseMap(0);
        return topDictData.EncodingOffset switch
        {
            0 => LogBuiltinAsync("Built-in Encoding Standard", bookmark),
            1 => LogBuiltinAsync("Built-in Encoding Expert", bookmark),
            _ => CustomEncodingAsync(bookmark)
        };
    }

    private async ValueTask CustomEncodingAsync(ParseMapBookmark? bookmark)
    {
        bookmark.IndentParseMap("Custom Encoding");
        bookmark.JumpToParseMap(0);
            ((await ReadCmapAsync(bookmark).CA()) as IDisposable )?.Dispose();
        bookmark.OutdentParseMap();
    }


#else
        public ValueTask TryAddToParseMapAsync() => ValueTask.CompletedTask;
#endif
}
