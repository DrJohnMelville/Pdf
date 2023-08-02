using Melville.INPC;
using Melville.Pdf.LowLevel.Encryption.SecurityHandlers;
using Melville.Pdf.LowLevel.Filters.FilterProcessing;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal partial class LazyCryptContextBuffer
{
    [FromConstructor] private readonly ParsingFileOwner owner;

    private int objectNumber = -1;
    private int generation = -1;
    private IObjectCryptContext? current = null;

    public void SetCurrentObject(int objectNumber, int generation)
    {
        this.objectNumber = objectNumber;
        this.generation = generation;
        current = null;
    }

    public void ClearObject() => SetCurrentObject(-1, -1);

    public IObjectCryptContext GetContext() =>
        current ??= CreateContext();

    private IObjectCryptContext CreateContext() =>
        objectNumber < 0 || generation < 0
            ? NullSecurityHandler.Instance
            : owner.CryptContextForObject(objectNumber, generation);
}