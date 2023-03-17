using System;

namespace Melville.JBig2.BinaryBitmaps;

internal readonly struct BinaryBitmapCopyDimension
{
    public int SrcBegin { get; }
    public int SrcExclusiveEnd { get; }
    public int DestBegin { get; }

    public BinaryBitmapCopyDimension(int srcOffset, int length, int srcMax, int destOffset, int destMax)
    {
        length = Math.Min(length, Math.Min(srcMax - srcOffset, destMax - destOffset));

        var neededOffset = Math.Min(srcOffset, destOffset);
        if (neededOffset < 0)
        {
            srcOffset -= neededOffset;
            destOffset -= neededOffset;
            length += neededOffset;
        }

        SrcBegin = srcOffset;
        SrcExclusiveEnd = srcOffset + length;
        DestBegin = destOffset;
    }

    public readonly int Length => SrcExclusiveEnd - SrcBegin;

    public readonly bool IsTrivial() => Length <= 0;
}

internal readonly struct BinaryBitmapCopyRegion
{
    public readonly BinaryBitmapCopyDimension Vertical;
    public readonly BinaryBitmapCopyDimension Horizontal;

    public BinaryBitmapCopyRegion(BinaryBitmapCopyDimension vertical, BinaryBitmapCopyDimension horizontal)
    {
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public readonly bool UseSlowAlgorithm => Horizontal.SrcExclusiveEnd - Horizontal.SrcBegin < 9;
    public readonly bool IsTrivial() => Horizontal.IsTrivial() || Vertical.IsTrivial();
}