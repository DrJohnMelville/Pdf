using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.Decryptors;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

public class InlineStreamSource: IStreamDataSource
{
    private long sourceFilePosition;
    private readonly ParsingFileOwner parsingFileOwner;
    private readonly IObjectCryptContext decryptor;
    public StreamFormat SourceFormat => StreamFormat.DiskRepresentation;

    public InlineStreamSource(
        long sourceFilePosition, ParsingFileOwner parsingFileOwner, IObjectCryptContext decryptor)
    {
        this.sourceFilePosition = sourceFilePosition;
        this.parsingFileOwner = parsingFileOwner;
        this.decryptor = decryptor;
    }

    public async ValueTask<Stream> OpenRawStream(long streamLength)
    {
        return await parsingFileOwner.RentStream(sourceFilePosition, streamLength);
    }

    public Stream WrapStreamWithDecryptor(Stream encryptedStream, PdfName cryptFilterName) => 
        decryptor.NamedCipher(cryptFilterName).Decrypt().CryptStream(encryptedStream);
    public Stream WrapStreamWithDecryptor(Stream encryptedStream) => 
        decryptor.StreamCipher().Decrypt().CryptStream(encryptedStream);
}