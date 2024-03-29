﻿using System;
using System.Buffers;
using Melville.Parsing.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;

namespace Melville.Pdf.LowLevel.Filters.AsciiHexFilters;

internal class AsciiHexEncoder : IStreamFilterDefinition
{
    public int MinWriteSize => 2;

    public (SequencePosition SourceConsumed, int bytesWritten, bool Done)
        Convert(ref SequenceReader<byte> source, in Span<byte> destination)
    {
        int i;
        for (i = 0; i + 1 < destination.Length && source.TryRead(out var inputByte); i += 2)
        {
            (destination[i], destination[i + 1]) = HexMath.CharPairFromByte(inputByte);
        }

        return (source.Position, Math.Min(i, destination.Length), false);
    }
}