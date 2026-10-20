using Melville.Parsing;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Postscript.Interpreter.Tokenizers;
using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Threading.Tasks;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers;

internal readonly struct XrefTableParser(ParsingBuffer buffer, IIndirectObjectRegistry registry)
{
    public async ValueTask ParseAsync()
    {
        while ((await buffer.ReadTextLongAsync().CA()) is { } first)
        {
            var blockLength = await buffer.ReadTextLongAsync().CA();
            if (!blockLength.HasValue) throw new PdfParseException("Invalid Xref Table");
            if (blockLength is 0) continue;
            await buffer.SkipToAsync(CharacterClassifier.DecimalDigitSearchValues).CA();
            await ReadTableValuesAsync((int)first, (int)blockLength.Value).CA();
        }
    }
    async ValueTask ReadTableValuesAsync(int first, int blockLength)
    {
        for (int i = 0; i < blockLength; i++)
        {
            await buffer.EnsureBytesAsync(20).CA();
            ParseSingleLine(buffer.Peek(), first+i);
            buffer.Advance(20);
        }  
    }
    void ParseSingleLine(Span<byte> peek, int objectNumber) => 
        HandleXrefLine(long.Parse(peek[..10]), int.Parse(peek[11..16]), peek[17], objectNumber);
    
    private void HandleXrefLine(long first, int second, byte operation, int objectNumber)
    {
        switch (operation)
        {
            case (byte)'n':
                registry.RegisterIndirectBlock(objectNumber, second, first);
                break;
            case (byte)'f':
                registry.RegisterDeletedBlock(objectNumber, (int)first, second);
                break;
            default: throw new PdfParseException("Invalid Xref Table Operation");
        }
    }
}