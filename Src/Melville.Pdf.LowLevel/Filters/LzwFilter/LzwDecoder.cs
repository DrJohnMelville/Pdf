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
        public ValueTask<Stream> WrapStreamAsync(Stream input, PdfObject parameter) =>
            ValueTask.FromResult<Stream>(new LzwDecodeWrapper(PipeReader.Create(input)));
        
        private class LzwDecodeWrapper: SequentialReadFilterStream
        {
            private readonly BitReader reader;
            private readonly DecoderDictionary dictionary = new DecoderDictionary();
            private short codeBeingWritten = -1;
            private int nextByteToWrite = int.MaxValue;
            private bool done;

            public LzwDecodeWrapper(PipeReader input)
            {
                reader = new BitReader(input);
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
                    var item = await reader.TryRead(9);
                    if (item is null or LzwConstants.EndOfFileCode)
                    {
                        done = true;
                        return destPosition;
                    }
                    if (item.Value == LzwConstants.ClearDictionaryCode) continue;  // todo implement clearing dict
                    HandleNewCodedGroup((short)item);
                }
            }

            private int TryWriteCurrentCode(Memory<byte> destination, int destPosition)
            {
                var localWrite = codeBeingWritten >= 0 ? WriteCurrentCodeToDestionation(destination, destPosition) : 0;
                destPosition += localWrite;
                nextByteToWrite += localWrite;
                return destPosition;
            }

            private void HandleNewCodedGroup(short item)
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
                ScheduleNewCodeForOutput(
                    dictionary.AddChild(codeBeingWritten, dictionary.FirstChar(codeBeingWritten)));
            }

            private void HandleKnownCode(short item)
            {
                if (codeBeingWritten >= 0)
                {
                    dictionary.AddChild(codeBeingWritten, dictionary.FirstChar(item));
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