﻿using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Postscript.Interpreter.Tokenizers;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers
{
    internal static class PdfNameTokenizer
    {
        public static bool Parse(ref SequenceReader<byte> reader, out PdfDirectObject result)
        {
            if (!reader.TryReadToAny(
                    out ReadOnlySpan<byte> text,CharacterClassifier.DelimiterChars(), false))
                return default(PdfDirectObject).AsFalseValue(out result);
            return Parse(text).AsTrueValue(out result);
        }
        public static PdfDirectObject Parse(ReadOnlySpan<byte> nameText)
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
                    var highByte = CharacterClassifier.ValueFromDigit(nameText[++srcPos]);
                    var lowByte = CharacterClassifier.ValueFromDigit(nameText[++srcPos]);
                    scratch[destPos++] = AssembleBytes(highByte, lowByte);
                }
                else
                    scratch[destPos++] = nameText[srcPos];
            }

            return PdfDirectObject.CreateName(scratch[..destPos]);
        }

        private static byte AssembleBytes(byte highNibble, byte lowNibble) => 
            (byte)((highNibble << 4) | lowNibble);
    }
}