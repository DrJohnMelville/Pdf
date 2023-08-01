using System.IO;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using PdfDirectValue = Melville.Pdf.LowLevel.Model.Objects.PdfDirectValue;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal class PdfFileStreamSource: IStreamDataSource
{
    private long sourceFilePosition;
    private readonly ParsingFileOwner parsingFileOwner;
    private readonly IObjectCryptContext decryptor;
    public StreamFormat SourceFormat => StreamFormat.DiskRepresentation;

    public PdfFileStreamSource(
        long sourceFilePosition, ParsingFileOwner parsingFileOwner, IObjectCryptContext decryptor)
    {
        this.sourceFilePosition = sourceFilePosition;
        this.parsingFileOwner = parsingFileOwner;
        this.decryptor = decryptor;
    }

    public async ValueTask<Stream> OpenRawStreamAsync(long streamLength)
    {
        return await parsingFileOwner.RentStreamAsync(sourceFilePosition, streamLength).CA();
    }

    public Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfDirectValue cryptFilterName) => 
        decryptor.NamedCipher(cryptFilterName).Decrypt().CryptStream(encryptedStream);
    public Stream WrapStreamWithDecryptor(Stream encryptedStream) => 
        decryptor.StreamCipher().Decrypt().CryptStream(encryptedStream);
}