using System;
using System.Collections.Generic;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public class TwoDimensionalLineCodeDictionary : ICodeDictionay
{
    private int horizontalCodesExpected = 0;
    public bool TryReadCode(in (int SourceBits, int BitLength) input, bool isWhiteRun, out CcittCode code)
    {
        if (horizontalCodesExpected > 0) return TryReadHorizontalCode(input, isWhiteRun, out code);
        if (!operationCodeBook.TryGetValue(input, out code)) return false;
        if (code.Operation == CcittCodeOperation.SwitchToHorizontalMode)
            RecordExpectedHorizontalCodes(out code);
        return true;
    }

    private void RecordExpectedHorizontalCodes(out CcittCode code)
    {
        code = new CcittCode(CcittCodeOperation.NoCode, 0);
        horizontalCodesExpected += 2;
    }

    private bool TryReadHorizontalCode((int SourceBits, int BitLength) input, bool isWhiteRun, out CcittCode code)
    {
        if (!inner.TryReadCode(input, isWhiteRun, out code)) return false;
        if (code.Operation != CcittCodeOperation.NoCode) horizontalCodesExpected--;
        return true;
    }

    private ICodeDictionay inner = new MakeUpExpander(TerminalCodeDictionary.Instance);
    private Dictionary<(int, int), CcittCode> operationCodeBook = new()
    {
        //Pass Mode
        {(4, 0b0001), new CcittCode(CcittCodeOperation.Pass, 0)},
        // verticalCodes
        {(1,0b1), new CcittCode(CcittCodeOperation.Vertical, 3)},
        {(3,0b011), new CcittCode(CcittCodeOperation.Vertical, 4)},
        {(6,0b000011), new CcittCode(CcittCodeOperation.Vertical, 5)},
        {(7,0b0000011), new CcittCode(CcittCodeOperation.Vertical, 6)},
        {(3,0b010), new CcittCode(CcittCodeOperation.Vertical, 2)},
        {(6,0b000010), new CcittCode(CcittCodeOperation.Vertical, 1)},
        {(7,0b0000010), new CcittCode(CcittCodeOperation.Vertical, 0)},
        //Transition to horizontalMode
        {(3, 0b001), new CcittCode(CcittCodeOperation.SwitchToHorizontalMode, 0)}
    };
}