using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

internal abstract partial class CMapMapperBase
{
    [FromConstructor] private readonly VariableBitChar minValue;
    [FromConstructor] private readonly VariableBitChar maxValue;
    #if DEBUG
        partial void OnConstructed()
        {
            Debug.Assert(minValue.Length() == maxValue.Length());
        }
    #endif

    public bool AppliesTo(VariableBitChar character) => 
        minValue <= character && character <= maxValue;

    protected uint OffsetFor(in VariableBitChar character) => (uint)(character - minValue);
    public int ByteLength() => minValue.Length();

    
    public abstract int WriteMapping(in VariableBitChar character, Span<uint> target);
}

[FromConstructor]
internal partial class ByteRange : CMapMapperBase
{
    private readonly List<CMapMapperBase> mappers = new();

    public ByteRange(VariableBitChar minValue, VariableBitChar maxValue,
        params CMapMapperBase[] mappers) 
        : this(minValue, maxValue)
    {
        this.mappers.AddRange(mappers);
    }

    public void AddMapper(CMapMapperBase mapper) => mappers.Add(mapper);

    public override int WriteMapping(in VariableBitChar character, Span<uint> target)
    {
        AssertValidRange();

        for (int i = mappers.Count -1; i >= 0; i--)
        {
            var candidate = mappers[i];
            if (candidate.AppliesTo(character)) 
                return candidate.WriteMapping(character, target);
        }
        return WriteNotDefinedMapping(target);
    }

    private void AssertValidRange()
    {
        if (mappers.Count == 0)
            throw new InvalidDataException("Cannot have an empty byte range");
    }

    private static int WriteNotDefinedMapping(Span<uint> target)
    {
        target[0] = 0;
        return 1;
    }
}