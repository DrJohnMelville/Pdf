using System.IO;
using System.Threading.Tasks;

using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Objects.StreamParts;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal class PdfFileStreamSource: IStreamDataSource
{
    private readonly long sourceFilePosition;
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

    public IPdfStreamReader OpenRawReader(long streamLength) =>
        parsingFileOwner.SubsetIAsyncReader(sourceFilePosition, streamLength);


    public IPdfStreamReader WrapStreamWithDecryptor(IPdfStreamReader encryptedStream, PdfDirectObject cryptFilterName) => 
        decryptor.NamedCipher(cryptFilterName).Decrypt().CryptStream(encryptedStream);
    public IPdfStreamReader WrapStreamWithDecryptor(IPdfStreamReader encryptedStream) => 
        decryptor.StreamCipher().Decrypt().CryptStream(encryptedStream);

}