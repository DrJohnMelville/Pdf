using System;
using Melville.INPC;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal partial class BaseFontConstantMapper : CMapMapperBase
{
    [FromConstructor] private readonly IReadCharacter baseFont;
    [FromConstructor] private readonly PostscriptValue mappedValue;

    public override int WriteMapping(in VariableBitChar character, Memory<uint> target)
    {
        var currentPosition = 0;
        using var source = mappedValue.Get<RentedMemorySource>();
        var inputMemory = source.Memory;
        while (inputMemory.Length > 0)
        {
            var outputChars = baseFont.GetCharacters(
                inputMemory, target[currentPosition..], out var bytesConsumed);
            inputMemory = inputMemory.Slice(bytesConsumed);
            currentPosition += outputChars.Length;
        }

        return currentPosition;
    }
}