﻿using System;
using Melville.INPC;

namespace Melville.Pdf.Model.Renderers.FontRenderings.CMaps;

[FromConstructor]
internal partial class BaseFontLinearMapper : BaseFontConstantMapper
{
    public override int WriteMapping(in VariableBitChar character, Span<uint> target)
    {
        var ret = base.WriteMapping(in character, target);
        if (ret > 0)
        {
            target[ret - 1] += OffsetFor(character);
        }
        return ret;
    }
}