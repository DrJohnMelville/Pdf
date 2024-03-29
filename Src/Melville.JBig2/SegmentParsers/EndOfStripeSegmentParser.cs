﻿using System.Buffers;
using Melville.JBig2.FileOrganization;
using Melville.JBig2.Segments;
using Melville.Parsing.SequenceReaders;

namespace Melville.JBig2.SegmentParsers;

internal static class EndOfStripeSegmentParser
{
    public static Segment Read(scoped in SegmentHeader header, scoped ref SequenceReader<byte> reader) => 
        new EndOfStripeSegment(reader.ReadBigEndianUint32());
}