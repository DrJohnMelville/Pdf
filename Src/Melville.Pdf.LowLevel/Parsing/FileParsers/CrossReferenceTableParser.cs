using System;
using System.Buffers;
using System.Diagnostics;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Parsing.ObjectParsers;
using Melville.Pdf.LowLevel.Parsing.ParserContext;

namespace Melville.Pdf.LowLevel.Parsing.FileParsers
{
    public class CrossReferenceTableParser
    {
        private readonly ParsingSource source;
        private readonly IIndirectObjectResolver target;

        public CrossReferenceTableParser(ParsingSource source, IIndirectObjectResolver target)
        {
            this.source = source;
            this.target = target;
        }

        public async Task Parse()
        {
            bool shouldContinue;
            do
            {
                var ret = await source.ReadAsync();
                shouldContinue = TryReadLine(ret.Buffer);
            } while (shouldContinue);
        }

        private bool TryReadLine(ReadOnlySequence<byte> input)
        {
            var reader = new SequenceReader<byte>(input);
            var consumedPoint = input.Start;
            while (reader.TryReadTo(out ReadOnlySequence<byte> line, (int) '\n'))
            {
                if (!ParseLine(line))
                {
                    source.AdvanceTo(consumedPoint);
                    return false;
                }

                consumedPoint = reader.Position;
            }
            source.AdvanceTo(consumedPoint, input.End);
            return true;
        }

        private int nextItem = 0;
        private bool ParseLine(ReadOnlySequence<byte> line)
        {
            var lineReader = new SequenceReader<byte>(line);
            if (!LineHasContent(ref lineReader, out var firstByte)) return false;
            if (IsXrefLine(firstByte)) return true; 
            if (!GatherTwoNumbers(ref lineReader, out var leftNum, out var rightNum, out var delim)) 
                return false;
            if (HandleAsGroupHeader(delim, leftNum)) return true;
            if (!lineReader.TryPeek(out byte operation)) return false; // empty line
            HandleObjectDeclarationLine(operation, rightNum, leftNum);
            return true;
        }

        private static bool LineHasContent(ref SequenceReader<byte> lineReader, out byte firstByte) => 
            lineReader.TryPeek(out firstByte);

        private static bool IsXrefLine(byte peeked) => peeked == 'x';

        private void HandleObjectDeclarationLine(byte operation, long rightNum, long leftNum)
        {
            if (operation == (byte) 'n') target.AddLocationHint(nextItem, (int)rightNum, leftNum);
            nextItem++;
        }

        private bool HandleAsGroupHeader(byte delim, long leftNum)
        {
            if (delim != 13) return false;
            nextItem = (int)leftNum;
            return true;
        }

        private static bool GatherTwoNumbers(
            ref SequenceReader<byte> lineReader, out long leftNum, out long rightNum, out byte delim)
        {
            rightNum = 0;
            return WholeNumberParser.TryParsePositiveWholeNumber(
                       ref lineReader, out leftNum, out delim) &&
                   WholeNumberParser.TryParsePositiveWholeNumber(
                       ref lineReader, out rightNum, out delim);
        }
    }
}