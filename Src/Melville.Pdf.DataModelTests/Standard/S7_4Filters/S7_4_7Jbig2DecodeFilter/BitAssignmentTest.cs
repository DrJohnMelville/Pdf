using Melville.JBig2.HuffmanTables;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Melville.Pdf.DataModelTests.Standard.S7_4Filters.S7_4_7Jbig2DecodeFilter;

public class BitAssignmentTest
{
    public static IEnumerable<(int[], int[])> BitAssignmentPatterns() =>
        [
            ([1,2,3,0,3], [0,2,6,0,7]),  // b1
            ([1,2,3,4,5,6,6], [0,0b10, 0b110,0b1110, 0b11110, 0b111110, 0b111111]),  // b2
            ([8,1,2,3,4,5,8,7,6], [0b11111110, 0, 0b10,0b110, 0b1110, 0b11110, 0xFF, 0b1111110, 0b111110]),  // b3
            ([1,2,3,4,5,5], [0,2,6, 14, 30, 31]),  // b4
            ([7,1,2,3,4,5,7,6], [126, 0, 2, 6, 14, 30, 127, 62]),  // b5
            ([5, 4, 4, 4, 5, 5, 4, 2,3,3,4,4,6,6], 
             [0b11100, 0b1000, 0b1001, 0b1010,0b11101, 0b11110, 0b1011, 0, 2, 3, 0b1100, 0b1101, 0b111110, 0b111111]),  // b6
            ([4,3,4,5,5,4,4,5,5,4,3,3,3,5,5],
             [0b1000, 0, 0b1001, 0b11010, 0b11011, 0b1010, 0b1011, 0b11100, 0b11101, 0b1100, 0b001, 0b010, 0b011, 0b11110, 0b11111]),  // b7
            ([8,9,8,9,7,4,2,5,6, 3,6,4,4,5,5,6,7,6,9,9,2],
              [0b11111100, 0b111111100, 0b11111101, 0b111111101, 0b1111100, 0b1010,0,0b11010, 0b111010, 0b100, 
                    0b111011, 0b1011, 0b1100, 0b11011, 0b11100, 0b111100, 0b1111101, 0b111101, 0b111111110, 0b111111111,0b01]),  // b8
            ([8,9,8,9,7,4,3,3,5,6,3,6,4,4,5,5,6,7,6,9,9,2],
             [0b11111100, 0b111111100, 0b11111101, 0b111111101, 0b1111100, 0b1010, 0b010, 0b011, 0b11010,
                    0b111010, 0b100, 0b111011, 0b1011, 0b1100, 0b11011, 0b11100, 0b111100, 0b1111101, 0b111101,
                    0b111111110, 0b111111111, 0b00 ]),  // b9
            ([8,9,8,9,7,4,3,3,5,6,3,6,4,4,5,5,6,7,6,9,9,2],
             [0b11111100, 0b111111100, 0b11111101, 0b111111101, 0b1111100, 0b1010, 0b010, 0b011, 0b11010,
                    0b111010, 0b100, 0b111011, 0b1011, 0b1100, 0b11011, 0b11100, 0b111100, 0b1111101, 0b111101,
                    0b111111110, 0b111111111, 0b00 ]),  // 10
        ];

    [Test]
    [MethodDataSource(nameof(BitAssignmentPatterns))]
    public void AssignBitLengths(int[] lengths, int[] result)
    {
        Assert.Equal(lengths.Length, result.Length);
        
        var codes = new int[result.Length];
        BitAssignment.AssignPrefixes(lengths, codes);
        for (int i = 0; i < lengths.Length; i++)
        {
            Assert.Equal(result[i], codes[i]);
            
        }
        Assert.SeqEqual(result, codes);
        
    }
}