using System;
using System.Buffers;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Primitives.VariableBitEncoding;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public class LzwDecodeFilter : IStreamFilterDefinition
    {
        private readonly BitReader reader;
        private readonly DecoderDictionary dictionary = new();
        private readonly BitLength bits;
        private short codeBeingWritten = EmptyCode;
        private int nextByteToWrite = int.MaxValue;
        private const short EmptyCode = -1;

        public int MinWriteSize => 1;

        public LzwDecodeFilter(int sizeSwitchFlavorDelta)
        {
            bits = new BitLength(9, sizeSwitchFlavorDelta);
            reader = new BitReader();
        }

        public (SequencePosition SourceConsumed, int bytesWritten, bool Done)
            Convert(ref SequenceReader<byte> source, ref Span<byte> destination)
        {
            var destPosition = 0;
            while (true)
            {
                destPosition = TryWriteCurrentCode(destination, destPosition);
                if (destPosition >= destination.Length)
                    return (source.Position, destination.Length, false);

                switch (reader.TryRead(bits.Length, ref source))
                {
                    case null: return (source.Position, destPosition, false);
                    case LzwConstants.EndOfFileCode:
                        return (source.Position, destPosition, true);
                    case LzwConstants.ClearDictionaryCode:
                        ResetDictionary();
                        break;
                    case var item:
                        HandleCodedGroup((short)item);
                        break;
                }
            }
        }

        public (SequencePosition SourceConsumed, int bytesWritten, bool Done)
            FinalConvert(ref SequenceReader<byte> source, ref Span<byte> destination) =>
            (source.Position, 0, false);

        private void ResetDictionary()
        {
            dictionary.Reset();
            bits.SetBitLength(9);
            codeBeingWritten = EmptyCode;
        }

        private int TryWriteCurrentCode(in Span<byte> destination, int destPosition)
        {
            if (codeBeingWritten == EmptyCode) return destPosition;
            var localWrite = WriteCurrentCodeToDestionation(destination, destPosition);
            destPosition += localWrite;
            nextByteToWrite += localWrite;
            return destPosition;
        }

        private void HandleCodedGroup(short item)
        {
            if (dictionary.IsDefined(item))
            {
                HandleKnownCode(item);
            }
            else
            {
                HandleUnknownCode();
            }
        }

        private void HandleUnknownCode()
        {
            var child = dictionary.AddChild(codeBeingWritten, dictionary.FirstChar(codeBeingWritten));
            CheckBitLength(child);
            ScheduleNewCodeForOutput(
                child);
        }

        private void CheckBitLength(short child)
        {
            // the decoder is always one code behind the encoder, so we add 1 to the switching logic.
            bits.CheckBitLength(child + 1);
        }

        private void HandleKnownCode(short item)
        {
            if (codeBeingWritten >= 0)
            {
                CheckBitLength(dictionary.AddChild(codeBeingWritten, dictionary.FirstChar(item)));
            }

            ScheduleNewCodeForOutput(item);
        }

        private void ScheduleNewCodeForOutput(short code)
        {
            codeBeingWritten = code;
            nextByteToWrite = 0;
        }

        private int WriteCurrentCodeToDestionation(in Span<byte> destination, int destPosition)
        {
            var target = destination[destPosition..];
            var localWrite = dictionary.WriteChars(codeBeingWritten, nextByteToWrite, ref target);
            return localWrite;
        }
    }
}