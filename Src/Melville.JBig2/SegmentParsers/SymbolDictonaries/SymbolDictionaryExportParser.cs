﻿using System;
using System.Buffers;
using Melville.JBig2.BinaryBitmaps;
using Melville.JBig2.EncodedReaders;
using Melville.JBig2.Segments;

namespace Melville.JBig2.SegmentParsers.SymbolDictonaries;

internal readonly struct SymbolDictionaryExportParser
{
    private readonly uint exportedSymbols;
    public uint NewSymbols { get; }

    public SymbolDictionaryExportParser(uint exportedSymbols, uint newSymbols)
    {
        this.NewSymbols = newSymbols;
        this.exportedSymbols = exportedSymbols;
    }

    public IBinaryBitmap[] ParseExportedArray(ref SequenceReader<byte> reader, IEncodedReader intReader,
        in ReadOnlySpan<IBinaryBitmap> newBitmaps, in ReadOnlySpan<Segment> importedSegs)
    {
        var currentIndex = 0;
        var ret = new IBinaryBitmap[exportedSymbols];
        var sorter = new BitmapSorter(importedSegs, newBitmaps);
        while (currentIndex < exportedSymbols)
        {
            var offset = intReader.ExportFlags(ref reader);
            for (int i = 0; i < offset; i++)
            {
                sorter.Next(); // discard skipped bitmaps
            }
            var length = intReader.ExportFlags(ref reader);
            for (int i = 0; i < length; i++)
            {
                ret[currentIndex++] = sorter.Next();
            }
        }
        return ret ;
    }
}

internal ref struct BitmapSorter
{
    private readonly ReadOnlySpan<Segment> segments;
    private readonly ReadOnlySpan<IBinaryBitmap> newBitmaps;
    private int currentSegment = -1;
    private int currentImage = 0;

    public BitmapSorter(in ReadOnlySpan<Segment> segments, in ReadOnlySpan<IBinaryBitmap> newBitmaps)
    {
        this.segments = segments;
        this.newBitmaps = newBitmaps;
        IncrementSegment();
    }

    public IBinaryBitmap Next()
    {
        while (currentImage >= CurrentSegment.Length) IncrementSegment();
        return CurrentSegment[currentImage++];
    }

    private ReadOnlySpan<IBinaryBitmap> CurrentSegment =>
        currentSegment >= segments.Length
            ? newBitmaps
            : ((DictionarySegment)segments[currentSegment]).ExportedSymbols.Span;

    private void IncrementSegment()
    {
        do
        {
            currentSegment++;
        } while (currentSegment < segments.Length &&
                 segments[currentSegment] is not DictionarySegment);

        currentImage = 0;
    }
}