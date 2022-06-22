using System;
using System.Buffers;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using Melville.INPC;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;

public ref partial struct GenericRegionRefinementAlgorithm
{
    [FromConstructor]private readonly IBinaryBitmap target;
    [FromConstructor]private readonly IBinaryBitmap reference;
    [FromConstructor]private readonly RefinementTemplateSet template;
    [FromConstructor]private readonly MQDecoder decoder;
    [FromConstructor] private readonly int ltpContext;
    private bool ltp = false;
    
    public void Read(ref SequenceReader<byte> source)
    {
        var incrementalTemplate = template.ToIncrementalTemplate();
        for (int row = 0; row < target.Height; row++)
        {
            incrementalTemplate.SetToPosition(reference, target, row, 0);
            var rowWriter = target.StartWritingAt(row, 0);
            TryReadLtpBit(ref source);
            for (int col = 0; col < target.Width; col++)
            {
                if (ltp && NinePixelsSame(row, col, out var value))
                    rowWriter.AssignBit(value);
                else
                    DecodePixel(ref source, incrementalTemplate.context, ref rowWriter);
                incrementalTemplate.Increment();
                rowWriter.Increment();
            }
        }
    }

    private bool NinePixelsSame(int row, int col, out bool result)
    {
        result = GetRefBit(row - 1, col - 1);

        return
            GetRefBit(row - 1, col) == result &&
            GetRefBit(row - 1, col + 1) == result &&
            GetRefBit(row, col - 1) == result &&
            GetRefBit(row, col) == result &&
            GetRefBit(row, col + 1) == result &&
            GetRefBit(row + 1, col - 1) == result &&
            GetRefBit(row + 1, col) == result &&
            GetRefBit(row + 1, col + 1) == result;
    }

    private bool GetRefBit(int row, int col) => reference.ContainsPixel(row, col) && reference[row, col];

    private void TryReadLtpBit(ref SequenceReader<byte> source)
    {
        if (ltpContext != 0 &&
            decoder.GetBit(ref source, ref template.ContextFor(ltpContext)) == 1)
            ltp = !ltp;
    }

    private void DecodePixel(ref SequenceReader<byte> source, int contextNum,
        ref BitRowWriter bitRowWriter)
    {
        ref var context = ref template.ContextFor(contextNum);
        var bit = decoder.GetBit(ref source, ref context);
        bitRowWriter.AssignBit(bit == 1);
    }
}