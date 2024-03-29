﻿using Melville.INPC;
using Melville.MVVM.WaitingServices;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevelViewerParts.LowLevelViewer.DocumentParts;

public readonly partial struct ParsePdfObjectsToView
{
    [FromConstructor] private readonly IWaitingService waiting;
    [FromConstructor] private readonly KeyValuePair<(int ObjectNumber, int Generation), PdfIndirectObject>[] sourceList;

    private const int maxSegmentLength = 1000;

    public async Task<DocumentPart[]> ParseItemElementsAsync()
    {
        return TooLongForPreloadedList()?
            CreateLazyLoadList()
            : await CreatePreloadedListAsync();
    }
    
    private bool TooLongForPreloadedList() => sourceList.Length > maxSegmentLength;

    private DocumentPart[] CreateLazyLoadList()
    {
        var listLen = ComputeDecimatedLength();
        var ret = new DocumentPart[listLen + 2];
        for (int i = 0; i < listLen; i++)
        {
            ret[i + 1] =
                new ItemLoader(MemoryForSegment(i));
        }
        return ret;
    }

    private Memory<KeyValuePair<(int ObjectNumber, int Generation), PdfIndirectObject>> MemoryForSegment(int i) => 
        sourceList.AsMemory(i * maxSegmentLength, SegmentLength(i));

    private int SegmentLength(int i) => 
        ItemAfterSegment(i) - (i * maxSegmentLength);

    private int ItemAfterSegment(int i) => Math.Min((i+1) * maxSegmentLength, sourceList.Length);

    private int ComputeDecimatedLength() => 
        (sourceList.Length + maxSegmentLength - 1) / maxSegmentLength;

    private async Task<DocumentPart[]> CreatePreloadedListAsync()
    {
        var items = new DocumentPart[sourceList.Length + 2];
        var creator = new ItemLoader(sourceList);
        await creator.FillMemoryWithPartsAsync(waiting, items.AsMemory(1..^1));
        return items;
    }
}