using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.ShortStrings;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter;

internal static class PostscriptFunctionParser
{
    public static async Task<PdfFunction> ParseAsync(PdfStream source)
    {
        var domain = await source.ReadIntervalsAsync(KnownNames.Domain).CA();
        var range = await source.ReadIntervalsAsync(KnownNames.Range).CA();
        var op = await new PostscriptLanguageParser(PipeReader.Create(
            await source.StreamContentAsync().CA())).ParseCompositeAsync().CA();
        return new PostscriptFunction(domain, range, op);
    }
}

internal readonly struct PostscriptLanguageParser
{
    private readonly PostscriptTokenizer reader;
    private readonly Stack<CompositeOperation> composites;

    public PostscriptLanguageParser(PipeReader reader) : this()
    {
        this.reader = new PostscriptTokenizer(reader);
        composites = new Stack<CompositeOperation>();
    }

    public async Task<CompositeOperation> ParseCompositeAsync()
    {
        var open = await reader.ReadTokenAsync().CA();
        if (open != PostScriptOperations.Open_Brace)
            throw new PdfParseException("Postscript function must start with {");
        var ret = await OpenCompositeOperationAsync().CA();
        return ret;
    }

    private async Task<CompositeOperation> OpenCompositeOperationAsync()
    {
        var ret = new CompositeOperation();
        composites.Push(ret);
        await ParseInsideCompositeAsync(ret).CA();
        return ret;
    }

    private async Task ParseInsideCompositeAsync(CompositeOperation ret)
    {
        CompositeOperation? ifBranch = null;
        CompositeOperation? elseBranch = null;
        while (true)
        {
            switch (await reader.ReadTokenAsync().CA(), ifBranch, elseBRanch: elseBranch)
            {
                case (PostScriptOperations.OperationOpen_Brace, null, null):
                    ifBranch = await OpenCompositeOperationAsync().CA();
                    break;
                case (PostScriptOperations.OperationOpen_Brace, not null, null):
                    elseBranch = await OpenCompositeOperationAsync().CA();
                    break;
                case (PostScriptOperations.OperationIfElse, { }ib, {}eb):
                    ret.AddOperation(new IfOperation(ib,eb));
                    ifBranch = elseBranch = null;
                    break;
                case (PostScriptOperations.OperationIf, {} ib, null):
                    ret.AddOperation(new IfOperation(ib));
                    ifBranch = null;
                    break;
                case (PostScriptOperations.OperationIf or PostScriptOperations.OperationIfElse, _,_):
                    throw new PdfParseException("If or ifelse must be preceeded by correct number of blocks");
                case (_,_,not null):
                case (_,not null, _):
                    throw new PdfParseException(
                        "Blocks in Postscript functions must be followed by if or ifelse");
                case (PostScriptOperations.OperationClose_Brace, null, null): return;
                case (var op1, null, null):
                    ret.AddOperation(op1);
                    break;
            }
        }
    }

    private void GenerateIfInstruction(CompositeOperation ret)
    {
        ret.AddOperation(new IfOperation(composites.Pop()));
    }

    private void GenerateIfElseInstruction(CompositeOperation ret)
    {
        var elseOp = composites.Pop();
        var ifOp = composites.Pop();
        ret.AddOperation(new IfOperation(ifOp, elseOp));
    }
}
    
internal readonly struct PostscriptTokenizer
{
    private readonly PipeReader reader;

    public PostscriptTokenizer(PipeReader reader)
    {
        this.reader = reader;
    }

    public async ValueTask<IPostScriptOperation> ReadTokenAsync()
    {
        while (true)
        {
            if (TryRead((await reader.ReadAsync().CA()), out var ret)) return ret;
        }
    }

    private bool TryRead(ReadResult readResult, out IPostScriptOperation result)
    {
        var sequenceReader = new SequenceReader<byte>(readResult.Buffer);
        var ret = TryRead(ref sequenceReader, out result);
        if (ret)
        {
            reader.AdvanceTo(sequenceReader.Position);
        }
        else
        {
            if (readResult.IsCompleted)
            {
                throw new PdfParseException("Ran out of input before end of PDF function");
            }
            reader.AdvanceTo(readResult.Buffer.Start, readResult.Buffer.End);
        }

        return ret;
    }
    private bool TryRead(ref SequenceReader<byte> reader, out IPostScriptOperation result)
    {
        result = PostScriptOperations.If;
        if (!reader.SkipToNextToken()) return false;
        if (!reader.TryPeek(out byte peeked)) return false;
        switch (peeked)
        {
            case (byte) '{':
                result = PostScriptOperations.Open_Brace;
                reader.Advance(1);
                return true;
            case (byte) '}':
                result = PostScriptOperations.Close_Brace;
                reader.Advance(1);
                return true;
            case (byte) '0':
            case (byte) '1':
            case (byte) '2':
            case (byte) '3':
            case (byte) '4':
            case (byte) '5':
            case (byte) '6':
            case (byte) '7':
            case (byte) '8':
            case (byte) '9':
            case (byte) '+':
            case (byte) '-':
                var numParser = new NumberWithFractionParser();
                if (!numParser.InnerTryParse(ref reader, false)) return false;
                result = new PushConstantOperation(numParser.DoubleValue());
                return true;
            default:
                var hash = new FnvComputer();
                while (true)
                {
                    if (!reader.TryPeek(out byte ret)) return false;
                    if (!IsChar(ret))
                    {
                        result = PostScriptOperationsDict.GetOperation(hash.HashAsUInt());
                        return true;
                    }
                    hash.SingleHashStep(ret);
                    reader.Advance(1);
                }
        }
    }
    private bool IsChar(byte value) => value is (>= (byte)'a' and <= (byte)'z');
}