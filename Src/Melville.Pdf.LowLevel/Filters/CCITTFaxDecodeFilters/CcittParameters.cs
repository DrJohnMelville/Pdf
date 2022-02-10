using System;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters;

public readonly struct CcittParameters
{
    public int K { get; }
    public bool EncodedByteAlign { get; }
    public int Columns { get; }
    public int Rows { get; }
    public bool EndOfBlock { get; }
    public bool BlackIs1 { get; }

    public CcittParameters(int k, bool encodedByteAlign, int columns, int rows, bool endOfBlock, bool blackIs1)
    {
        K = k;
        EncodedByteAlign = encodedByteAlign;
        Columns = columns;
        Rows = rows;
        EndOfBlock = endOfBlock;
        BlackIs1 = blackIs1;
    }

    public static ValueTask<CcittParameters> FromPdfObject(PdfObject? parameters) =>
        FromDictionary(parameters as PdfDictionary ?? PdfDictionary.Empty);
    public static async ValueTask<CcittParameters> FromDictionary(PdfDictionary parameters) =>
        new((int)await parameters.GetOrDefaultAsync(KnownNames.K, 0).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.EncodedByteAlign, false).CA(),
            (int)await parameters.GetOrDefaultAsync(KnownNames.Columns, 1728).CA(),
            (int)await parameters.GetOrDefaultAsync(KnownNames.Rows, 0).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.EndOfBlock, true).CA(),
            await parameters.GetOrDefaultAsync(KnownNames.BlackIs1, false).CA()
        );

    public bool[] CreateWhiteRow()
    {
        var ret = new bool[Columns];
        ret.AsSpan().Fill(WhiteValue);
        return ret;
    }

    public bool WhiteValue => !BlackIs1;
    public bool BlackValue => BlackIs1;

    public bool IsWhiteValue(bool value) => value != BlackValue;
}