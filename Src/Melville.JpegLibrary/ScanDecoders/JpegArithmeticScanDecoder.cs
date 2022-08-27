using System.Diagnostics;
using Melville.JpegLibrary.ArithmeticDecoding;
using Melville.JpegLibrary.Decoder;
using Melville.JpegLibrary.Readers;

namespace Melville.JpegLibrary.ScanDecoders;

internal abstract class JpegArithmeticScanDecoder : JpegScanDecoder
{
    protected JpegDecoder Decoder { get; private set; }

    public JpegArithmeticScanDecoder(JpegDecoder decoder)
    {
        Decoder = decoder;

        Reset();
    }

    private int _c;
    private int _a;
    private int _ct;

    private byte[] _fixedBin = new byte[4] { 113, 0, 0, 0 };

    private List<JpegArithmeticStatistics> _statistics = new List<JpegArithmeticStatistics>();

    protected ref byte GetFixedBinReference() => ref _fixedBin[0];

    private JpegArithmeticStatistics CreateOrGetStatisticsBin(bool dc, byte identifier, bool reset = false)
    {
        foreach (JpegArithmeticStatistics item in _statistics)
        {
            if (item.IsDcStatistics == dc && item.Identifier == identifier)
            {
                if (reset)
                {
                    item.Reset();
                }
                return item;
            }
        }
        var statistics = new JpegArithmeticStatistics(dc, identifier);
        _statistics.Add(statistics);
        return statistics;
    }

    protected int InitDecodeComponents(JpegFrameHeader frameHeader, JpegScanHeader scanHeader, Span<JpegArithmeticDecodingComponent> components)
    {
        Debug.Assert(frameHeader.Components is not null);
        Debug.Assert(scanHeader.Components is not null);

        // Compute maximum sampling factor
        int maxHorizontalSampling = 1;
        int maxVerticalSampling = 1;
        foreach (JpegFrameComponentSpecificationParameters currentFrameComponent in frameHeader.Components!)
        {
            maxHorizontalSampling = Math.Max(maxHorizontalSampling, currentFrameComponent.HorizontalSamplingFactor);
            maxVerticalSampling = Math.Max(maxVerticalSampling, currentFrameComponent.VerticalSamplingFactor);
        }

        // Resolve each component
        if (components.Length < scanHeader.NumberOfComponents)
        {
            throw new InvalidOperationException();
        }
        for (int i = 0; i < scanHeader.NumberOfComponents; i++)
        {
            JpegScanComponentSpecificationParameters scanComponenet = scanHeader.Components![i];
            int componentIndex = 0;
            JpegFrameComponentSpecificationParameters? frameComponent = null;

            for (int j = 0; j < frameHeader.NumberOfComponents; j++)
            {
                JpegFrameComponentSpecificationParameters currentFrameComponent = frameHeader.Components[j];
                if (scanComponenet.ScanComponentSelector == currentFrameComponent.Identifier)
                {
                    componentIndex = j;
                    frameComponent = currentFrameComponent;
                }
            }
            if (frameComponent is null)
            {
                ThrowInvalidDataException("The specified component is missing.");
            }
            JpegArithmeticDecodingComponent component = components[i];
            if (component is null)
            {
                components[i] = component = new JpegArithmeticDecodingComponent();
            }
            JpegArithmeticDecodingTable? dcTable = Decoder.GetArithmeticTable(true, scanComponenet.DcEntropyCodingTableSelector); ;
            JpegArithmeticDecodingTable? acTable = Decoder.GetArithmeticTable(false, scanComponenet.AcEntropyCodingTableSelector);
            component.ComponentIndex = componentIndex;
            component.HorizontalSamplingFactor = frameComponent.GetValueOrDefault().HorizontalSamplingFactor;
            component.VerticalSamplingFactor = frameComponent.GetValueOrDefault().VerticalSamplingFactor;
            component.DcTable = dcTable;
            component.AcTable = acTable;
            component.QuantizationTable = Decoder.GetQuantizationTable(frameComponent.GetValueOrDefault().QuantizationTableSelector);
            component.HorizontalSubsamplingFactor = maxHorizontalSampling / component.HorizontalSamplingFactor;
            component.VerticalSubsamplingFactor = maxVerticalSampling / component.VerticalSamplingFactor;
            component.DcPredictor = 0;
            component.DcContext = 0;
            component.DcStatistics = dcTable is null ? null : CreateOrGetStatisticsBin(true, dcTable.Identifier);
            component.AcStatistics = acTable is null ? null : CreateOrGetStatisticsBin(false, acTable.Identifier);
        }

        return scanHeader.NumberOfComponents;
    }

    protected JpegArithmeticDecodingComponent[] InitDecodeComponents(JpegFrameHeader frameHeader, JpegScanHeader scanHeader)
    {
        JpegArithmeticDecodingComponent[] components = new JpegArithmeticDecodingComponent[scanHeader.NumberOfComponents];
        InitDecodeComponents(frameHeader, scanHeader, components);
        return components;
    }

    protected int DecodeBinaryDecision(ref JpegBitReader reader, ref byte st)
    {
        byte nl, nm;
        int qe, temp;
        int sv;

        // Renormalization & data input per section D.2.6
        while (_a < 0x8000)
        {
            if (--_ct < 0)
            {
                // Need to fetch next data byte
                _ = reader.TryReadBits(8, out int data, out _);
                _c = _c << 8 | data; // insert data into C register
                if ((_ct += 8) < 0) // update bit shift counter
                {
                    // Need more initial bytes
                    if (++_ct == 0)
                    {
                        // Got 2 initial bytes -> re-init A and exit loop
                        _a = 0x8000; // e->a = 0x10000L after loop exit
                    }
                }
            }
            _a <<= 1;
        }

        // Fetch values from our compact representation of Table D.3(D.2):
        // Qe values and probability estimation state machine
        sv = st;
        qe = s_arithmeticTable[sv & 0x7f];
        nl = (byte)qe; qe >>= 8;	// Next_Index_LPS + Switch_MPS
        nm = (byte)qe; qe >>= 8;    // Next_Index_MPS

        // Decode & estimation procedures per sections D.2.4 & D.2.5
        temp = _a - qe;
        _a = temp;
        temp <<= _ct;
        if (_c >= temp)
        {
            _c -= temp;
            // Conditional LPS (less probable symbol) exchange
            if (_a < qe)
            {
                _a = qe;
                st = (byte)((sv & 0x80) ^ nm); // Estimate_after_MPS
            }
            else
            {
                _a = qe;
                st = (byte)((sv & 0x80) ^ nl); // Estimate_after_LPS
                sv ^= 0x80; // Exchange LPS/MPS
            }
        }
        else if (_a < 0x8000)
        {
            // Conditional MPS (more probable symbol) exchange
            if (_a < qe)
            {
                st = (byte)((sv & 0x80) ^ nl); // Estimate_after_LPS
                sv ^= 0x80;
            }
            else
            {
                st = (byte)((sv & 0x80) ^ nm); // Estimate_after_MPS
            }
        }

        return sv >> 7;
    }

    protected void Reset()
    {
        _c = 0;
        _a = 0;
        _ct = -16; // force reading 2 initial bytes to fill C
    }

    // The following function specifies the packing of the four components
    // into the compact INT32 representation.
    // Note that this formula must match the actual arithmetic encoder
    // and decoder implementation.  The implementation has to be changed
    // if this formula is changed.
    // The current organization is leaned on Markus Kuhn's JBIG
    // implementation (jbig_tab.c).
    private static int Pack(int i, int a, int b, int c, int d)
        => a << 16 | c << 8 | d << 7 | b;

    private static readonly int[] s_arithmeticTable = new int[]
    {
        Pack(   0, 0x5a1d,   1,   1, 1 ),
        Pack(   1, 0x2586,  14,   2, 0 ),
        Pack(   2, 0x1114,  16,   3, 0 ),
        Pack(   3, 0x080b,  18,   4, 0 ),
        Pack(   4, 0x03d8,  20,   5, 0 ),
        Pack(   5, 0x01da,  23,   6, 0 ),
        Pack(   6, 0x00e5,  25,   7, 0 ),
        Pack(   7, 0x006f,  28,   8, 0 ),
        Pack(   8, 0x0036,  30,   9, 0 ),
        Pack(   9, 0x001a,  33,  10, 0 ),
        Pack(  10, 0x000d,  35,  11, 0 ),
        Pack(  11, 0x0006,   9,  12, 0 ),
        Pack(  12, 0x0003,  10,  13, 0 ),
        Pack(  13, 0x0001,  12,  13, 0 ),
        Pack(  14, 0x5a7f,  15,  15, 1 ),
        Pack(  15, 0x3f25,  36,  16, 0 ),
        Pack(  16, 0x2cf2,  38,  17, 0 ),
        Pack(  17, 0x207c,  39,  18, 0 ),
        Pack(  18, 0x17b9,  40,  19, 0 ),
        Pack(  19, 0x1182,  42,  20, 0 ),
        Pack(  20, 0x0cef,  43,  21, 0 ),
        Pack(  21, 0x09a1,  45,  22, 0 ),
        Pack(  22, 0x072f,  46,  23, 0 ),
        Pack(  23, 0x055c,  48,  24, 0 ),
        Pack(  24, 0x0406,  49,  25, 0 ),
        Pack(  25, 0x0303,  51,  26, 0 ),
        Pack(  26, 0x0240,  52,  27, 0 ),
        Pack(  27, 0x01b1,  54,  28, 0 ),
        Pack(  28, 0x0144,  56,  29, 0 ),
        Pack(  29, 0x00f5,  57,  30, 0 ),
        Pack(  30, 0x00b7,  59,  31, 0 ),
        Pack(  31, 0x008a,  60,  32, 0 ),
        Pack(  32, 0x0068,  62,  33, 0 ),
        Pack(  33, 0x004e,  63,  34, 0 ),
        Pack(  34, 0x003b,  32,  35, 0 ),
        Pack(  35, 0x002c,  33,   9, 0 ),
        Pack(  36, 0x5ae1,  37,  37, 1 ),
        Pack(  37, 0x484c,  64,  38, 0 ),
        Pack(  38, 0x3a0d,  65,  39, 0 ),
        Pack(  39, 0x2ef1,  67,  40, 0 ),
        Pack(  40, 0x261f,  68,  41, 0 ),
        Pack(  41, 0x1f33,  69,  42, 0 ),
        Pack(  42, 0x19a8,  70,  43, 0 ),
        Pack(  43, 0x1518,  72,  44, 0 ),
        Pack(  44, 0x1177,  73,  45, 0 ),
        Pack(  45, 0x0e74,  74,  46, 0 ),
        Pack(  46, 0x0bfb,  75,  47, 0 ),
        Pack(  47, 0x09f8,  77,  48, 0 ),
        Pack(  48, 0x0861,  78,  49, 0 ),
        Pack(  49, 0x0706,  79,  50, 0 ),
        Pack(  50, 0x05cd,  48,  51, 0 ),
        Pack(  51, 0x04de,  50,  52, 0 ),
        Pack(  52, 0x040f,  50,  53, 0 ),
        Pack(  53, 0x0363,  51,  54, 0 ),
        Pack(  54, 0x02d4,  52,  55, 0 ),
        Pack(  55, 0x025c,  53,  56, 0 ),
        Pack(  56, 0x01f8,  54,  57, 0 ),
        Pack(  57, 0x01a4,  55,  58, 0 ),
        Pack(  58, 0x0160,  56,  59, 0 ),
        Pack(  59, 0x0125,  57,  60, 0 ),
        Pack(  60, 0x00f6,  58,  61, 0 ),
        Pack(  61, 0x00cb,  59,  62, 0 ),
        Pack(  62, 0x00ab,  61,  63, 0 ),
        Pack(  63, 0x008f,  61,  32, 0 ),
        Pack(  64, 0x5b12,  65,  65, 1 ),
        Pack(  65, 0x4d04,  80,  66, 0 ),
        Pack(  66, 0x412c,  81,  67, 0 ),
        Pack(  67, 0x37d8,  82,  68, 0 ),
        Pack(  68, 0x2fe8,  83,  69, 0 ),
        Pack(  69, 0x293c,  84,  70, 0 ),
        Pack(  70, 0x2379,  86,  71, 0 ),
        Pack(  71, 0x1edf,  87,  72, 0 ),
        Pack(  72, 0x1aa9,  87,  73, 0 ),
        Pack(  73, 0x174e,  72,  74, 0 ),
        Pack(  74, 0x1424,  72,  75, 0 ),
        Pack(  75, 0x119c,  74,  76, 0 ),
        Pack(  76, 0x0f6b,  74,  77, 0 ),
        Pack(  77, 0x0d51,  75,  78, 0 ),
        Pack(  78, 0x0bb6,  77,  79, 0 ),
        Pack(  79, 0x0a40,  77,  48, 0 ),
        Pack(  80, 0x5832,  80,  81, 1 ),
        Pack(  81, 0x4d1c,  88,  82, 0 ),
        Pack(  82, 0x438e,  89,  83, 0 ),
        Pack(  83, 0x3bdd,  90,  84, 0 ),
        Pack(  84, 0x34ee,  91,  85, 0 ),
        Pack(  85, 0x2eae,  92,  86, 0 ),
        Pack(  86, 0x299a,  93,  87, 0 ),
        Pack(  87, 0x2516,  86,  71, 0 ),
        Pack(  88, 0x5570,  88,  89, 1 ),
        Pack(  89, 0x4ca9,  95,  90, 0 ),
        Pack(  90, 0x44d9,  96,  91, 0 ),
        Pack(  91, 0x3e22,  97,  92, 0 ),
        Pack(  92, 0x3824,  99,  93, 0 ),
        Pack(  93, 0x32b4,  99,  94, 0 ),
        Pack(  94, 0x2e17,  93,  86, 0 ),
        Pack(  95, 0x56a8,  95,  96, 1 ),
        Pack(  96, 0x4f46, 101,  97, 0 ),
        Pack(  97, 0x47e5, 102,  98, 0 ),
        Pack(  98, 0x41cf, 103,  99, 0 ),
        Pack(  99, 0x3c3d, 104, 100, 0 ),
        Pack( 100, 0x375e,  99,  93, 0 ),
        Pack( 101, 0x5231, 105, 102, 0 ),
        Pack( 102, 0x4c0f, 106, 103, 0 ),
        Pack( 103, 0x4639, 107, 104, 0 ),
        Pack( 104, 0x415e, 103,  99, 0 ),
        Pack( 105, 0x5627, 105, 106, 1 ),
        Pack( 106, 0x50e7, 108, 107, 0 ),
        Pack( 107, 0x4b85, 109, 103, 0 ),
        Pack( 108, 0x5597, 110, 109, 0 ),
        Pack( 109, 0x504f, 111, 107, 0 ),
        Pack( 110, 0x5a10, 110, 111, 1 ),
        Pack( 111, 0x5522, 112, 109, 0 ),
        Pack( 112, 0x59eb, 112, 111, 1 ),

        // This last entry is used for fixed probability estimate of 0.5
        // as suggested in Section 10.3 Table 5 of ITU-T Rec. T.851.
        Pack( 113, 0x5a1d, 113, 113, 0 )
    };
}