using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal static class PdfNameTokenizer
{   public static PdfDirectObject Parse(ReadOnlySpan<byte> nameText)
    {
        if (!nameText.Contains((byte)'#'))
           return PdfDirectObject.CreateName(nameText);
        return ParseNameWithHashMark(nameText);
    }

    private static PdfDirectObject ParseNameWithHashMark(ReadOnlySpan<byte> nameText)
    {
        Span<byte> scratch = stackalloc byte[nameText.Length];
        var destPos = 0;
        for (int srcPos = 0; srcPos < nameText.Length; srcPos++)
        {
            if (nameText[srcPos] == (byte)'#' && srcPos + 2 < nameText.Length)
            {
                scratch[destPos++] =
                    CharacterClassifier.ByteFromHexChars(nameText[++srcPos], nameText[++srcPos]);
            }
            else
                scratch[destPos++] = nameText[srcPos];
        }

        return PdfDirectObject.CreateName(scratch[..destPos]);
    }

}