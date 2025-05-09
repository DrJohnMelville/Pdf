using System;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal partial class BaseFontConstantMapper : CMapMapperBase
{
    [FromConstructor] private readonly IReadCharacter baseFont;
    [FromConstructor] private readonly PostscriptValue mappedValue;

    public override int WriteMapping(in VariableBitChar character, Span<uint> target)
    {
        var currentPosition = 0;
        using var source = mappedValue.Get<RentedMemorySource>();
        var inputSpan = source.Memory.Span;
        while (inputSpan.Length > 0)
        {
            var outputChars = baseFont.GetCharacters(
                inputSpan, target[currentPosition..], out var bytesConsumed);
            if (bytesConsumed < 0) return -1;
            inputSpan = inputSpan.Slice(bytesConsumed);
            currentPosition += outputChars.Length;
        }

        return currentPosition;
    }
}