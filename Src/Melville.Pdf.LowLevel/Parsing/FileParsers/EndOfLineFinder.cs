using System.Buffers;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal static class EndOfLineFinder
{
    private static readonly byte[] EndOfLineMarkers = {10, 13};

    public static bool TrySkipToEndOfLineMarker(this ref SequenceReader<byte> reader) => 
        reader.TryAdvanceToAny(EndOfLineMarkers, false);
}