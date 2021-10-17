using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions.FunctionParser;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;

namespace Melville.Pdf.LowLevel.Model.Wrappers.Functions.PostScriptInterpreter
{
    public static class PostscriptFunctionParser
    {
        public static async Task<PdfFunction> Parse(PdfStream source)
        {
            var domain = await source.ReadIntervals(KnownNames.Domain);
            var range = await source.ReadIntervals(KnownNames.Range);
            var op = await new PostscriptLanguageParser(PipeReader.Create(
                await source.StreamContentAsync())).ParseCompositeAsync();
            return new PostscriptFunction(domain, range, op);
        }
    }

    public readonly struct PostscriptLanguageParser
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
            var open = await reader.ReadToken();
            if (open != PostScriptSpecialOperations.OpenBrace)
                throw new PdfParseException("Postscript function must start with {");
            var ret = new CompositeOperation();
            composites.Push(ret);
            await ParseInsideComposite(ret);
            return ret;
        }

        private async Task ParseInsideComposite(CompositeOperation ret)
        {
            while (true)
            {
                var op = await reader.ReadToken();
                if (op == PostScriptSpecialOperations.CloseBrace) return;
                ret.AddOperation(op);
            }
        }
    }
    
    public readonly struct PostscriptTokenizer
    {
        private readonly PipeReader reader;

        public PostscriptTokenizer(PipeReader reader)
        {
            this.reader = reader;
        }

        public async ValueTask<IPostScriptOperation> ReadToken()
        {
            while (true)
            {
                if (TryRead((await reader.ReadAsync()).Buffer, out var ret)) return ret;
            }
        }

        private bool TryRead(ReadOnlySequence<byte> buffer, out IPostScriptOperation result)
        {
            var sequenceReader = new SequenceReader<byte>(buffer);
            var ret = TryRead(ref sequenceReader, out result);
            if (ret)
            {
                reader.AdvanceTo(sequenceReader.Position);
            }
            else
            {
                reader.AdvanceTo(buffer.Start, buffer.End);
            }

            return ret;
        }
        private bool TryRead(ref SequenceReader<byte> reader, out IPostScriptOperation result)
        {
            result = PostScriptSpecialOperations.OutOfChars;
            if (!reader.SkipToNextToken()) return false;
            if (!reader.TryPeek(out byte peeked)) return false;
            switch (peeked)
            {
                case (byte) '{':
                    result = PostScriptSpecialOperations.OpenBrace;
                    reader.Advance(1);
                    return true;
                case (byte) '}':
                    result = PostScriptSpecialOperations.CloseBrace;
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
                    var numParser = new NumberWtihFractionParser();
                    if (!numParser.InnerTryParse(ref reader, false)) return false;
                    result = new PushConstantOperation(numParser.DoubleValue());
                    return true;
                default:
                    var hash = FnvHash.EmptyStringHash();
                    while (true)
                    {
                        if (!reader.TryPeek(out byte ret)) return false;
                        if (!IsChar(ret))
                        {
                            result = PostScriptOperationsDict.GetOperation(hash);
                            return true;
                        }
                        hash = FnvHash.SingleHashStep(hash, ret);
                        reader.Advance(1);
                    }
            }
        }
        private bool IsChar(byte value) => value is (>= (byte)'a' and <= (byte)'z');
    }
}