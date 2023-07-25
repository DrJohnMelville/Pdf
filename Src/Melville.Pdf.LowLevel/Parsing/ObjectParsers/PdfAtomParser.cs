using System;
using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.CountingReaders;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects2;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal abstract class PdfAtomParser : IPdfObjectParser
{
    public abstract bool TryParse(
        ref SequenceReader<byte> reader, bool final, IParsingReader source, [NotNullWhen(true)] out PdfObject? obj);

    public async Task<PdfObject> ParseAsync(IParsingReader source)
    {
        PdfObject result;
        do{}while(source.Reader.ShouldContinue(Parse(await source.Reader.ReadAsync().CA(), source, out result!)));
        return result;
    }

    private (bool Success, SequencePosition Position) Parse(
        ReadResult source, IParsingReader parsingReader, out PdfObject? result)
    {
        var reader = new SequenceReader<byte>(source.Buffer);
        return (TryParse(ref reader, source.IsCompleted, parsingReader, out result), reader.Position);
    }
}

internal static class DecryptStringOperation
{
    public static PdfDirectValue CreateDecryptedString(this IParsingReader reader, byte[] text) =>
        PdfDirectValue.CreateName(
            reader.ObjectCryptContext().StringCipher().Decrypt().CryptSpan(text));
}