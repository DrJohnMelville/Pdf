using Melville.JBig2.FileOrganization;
using Melville.Pdf.ReferenceDocuments.Utility;
using Melville.StreamInterfaces.Memory;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class SegmentHeaderParserTest
{

    public static IEnumerable<(byte[], object)> SegmentHeaderExamples() =>
    [
        ("00000020 86 6B 02 1E 05 04 00001234".BitsFromHex(),
            new SegmentHeader(32, SegmentType.ImmediateTextRegion, 4, 0x1234, new uint[]{2,30,5})
        ),
        (
            ("00000234" +// id number
            "40" + // flags
            "e0 00 00 09" + // 9 referred segments in long format
            "02 fd" + // retain bits
            "0100 0002 001E 0005 0200 0201 0202 0203 0204" + // referred segments
            "00 00 04 01" + // page number
            "12345678") // segment length
                .BitsFromHex(),
            new SegmentHeader(564, SegmentType.SymbolDictionary, 1025, 0x12345678, 
                new uint[]{256,2,0x1e, 5, 0x200, 0x201, 0x202, 0x203, 0x204})
        ),
        
        (@"0F00023440e000000902fd00000100000000020000001E0000000500000200000002010000020200000203000002040000040112345678".BitsFromHex(),
            new SegmentHeader(0x0f000234, SegmentType.SymbolDictionary, 1025, 0x12345678, 
                new uint[]{256,2,0x1e, 5, 0x200, 0x201, 0x202, 0x203, 0x204})
        )
    ];

    [Test]
    [MethodDataSource(nameof(SegmentHeaderExamples))]
    public async Task ReadSegment2ReadSegmentAsync(byte[] data, object result1)
    {
        var result = (SegmentHeader)result1;
        var reader = new MemoryReader(data);
        using var buffer = new ParsingBuffer(reader);
        var header = await SegmentHeaderParser.ParseAsync(buffer);

        Assert.Equal(result.Number, header.Number);
        Assert.Equal(result.SegmentType, header.SegmentType);
        Assert.SeqEqual(result.ReferencedSegmentNumbers, header.ReferencedSegmentNumbers);
        Assert.Equal(result.Page, header.Page);
        Assert.Equal(result.DataLength, header.DataLength);

    }
}