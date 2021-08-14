using System;
using System.Buffers;
using System.IO;
using System.IO.Pipelines;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public class LzwDecoder : IDecoder
    {
        public async ValueTask<Stream> WrapStreamAsync(Stream input, PdfObject parameter) =>
            new LzwDecodeWrapper(PipeReader.Create(input), await parameter.EarlySwitchLength());

        private class LzwDecodeWrapper: SequentialReadFilterStream
        {
            private readonly BitReader reader;
            private readonly DecoderDictionary dictionary = new();
            private readonly BitLength bits;
            private short codeBeingWritten = EmptyCode;
            private int nextByteToWrite = int.MaxValue;
            private bool done;
            private const short EmptyCode = -1;

            public LzwDecodeWrapper(PipeReader input, int sizeSwitchFlavorDelta)
            {
                bits = new BitLength(9, sizeSwitchFlavorDelta);
                reader = new BitReader(input);
            }

            protected override void Dispose(bool disposing)
            {
                reader.Dispose();
            }

            public async override ValueTask<int> ReadAsync(
                Memory<byte> destination, CancellationToken cancellationToken = default)
            {
                if (done) return 0;
                var destPosition = 0;
                while (true)
                {
                    
                    destPosition = TryWriteCurrentCode(destination, destPosition);
                    if (destPosition >= destination.Length) return destination.Length;
                    var item = await reader.TryRead(bits.Length);
                    if (item is null or LzwConstants.EndOfFileCode)
                    {
                        done = true;
                        return destPosition;
                    }

                    if (item.Value == LzwConstants.ClearDictionaryCode)
                    {
                        ResetDictionary();
                    }
                    else
                    {
                        HandleCodedGroup((short)item);
                    }
                }
            }

            private void ResetDictionary()
            {
                dictionary.Reset();
                bits.SetBitLength(9);
                codeBeingWritten = EmptyCode;
            }

            private int TryWriteCurrentCode(Memory<byte> destination, int destPosition)
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
                bits.CheckBitLength(child+1);
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

            private int WriteCurrentCodeToDestionation(Memory<byte> destination, int destPosition)
            {
                var target = destination.Slice(destPosition).Span;
                var localWrite = dictionary.WriteChars(codeBeingWritten, nextByteToWrite, ref target);
                return localWrite;
            }
        }
    }
}