using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.Decryptors;

internal interface IWrapReaderForDecryption
{
    IParsingReader Wrap(IParsingReader reader, int objectNumber, int generationNumber);
}