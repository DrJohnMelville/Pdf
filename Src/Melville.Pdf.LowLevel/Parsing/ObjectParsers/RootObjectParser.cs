using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Parsing.ParserContext;
using Melville.Postscript.Interpreter.Values;

namespace Melville.Pdf.LowLevel.Parsing.ObjectParsers;

internal readonly struct RootObjectParser
{ 
    private readonly PdfParsingStack stack;
    private readonly PdfTokenizer tokenizer;
    
    public RootObjectParser(ParsingReader source)
    {
        stack = new PdfParsingStack(source);
        tokenizer = new PdfTokenizer(source.Reader);
    }

    public async ValueTask<PdfDirectObject> ParseTopLevelObjectAsync()
    {
        stack.PushRootSignal();
        return await (await ParseAsync().CA()).LoadValueAsync().CA();
    }

    public async ValueTask<PdfIndirectObject> ParseAsync()
    {
        Debug.Assert(stack.Count == 0 || 
            (stack.HasRootSignal() && stack.Count == 1) );
        do
        {
            var token = await tokenizer.NextTokenAsync().CA();
            await ProcessTokenAsync(token).CA();
        } while (stack.Count != 1 || stack[0].IsPdfParsingOperation());
        return stack.Pop();
    }

    private ValueTask ProcessTokenAsync(PdfDirectObject token)
    {
        if (token.IsPdfParsingOperation())
            return token.TryExecutePdfParseOperationAsync(stack);
        if (token.IsString)
            token = DecryptString(token);
        
        stack.Push(token);
        return ValueTask.CompletedTask;

    }

    private PdfDirectObject DecryptString(PdfDirectObject token)
    {
        #warning need a shortcut for null strings to pass through quickly
        var decryptedSpan = stack.CryptoContext().StringCipher().Decrypt().CryptSpan(
            token.Get<StringSpanSource>().GetSpan());
        return PdfDirectObject.CreateString(decryptedSpan);
    }
}