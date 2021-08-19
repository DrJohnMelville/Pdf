using System;
using System.Collections.Generic;

namespace Melville.Pdf.LowLevel.Filters.StreamFilters
{
    public class MultiBuffer
    {
        private readonly List<byte[]> blocks = new();
        public long Length { get; private set; }
        private readonly int blockLength;

        public MultiBuffer(int blockLength)
        {
            if (blockLength < 1)
                throw new ArgumentException("Buffer length must be > 0");
            this.blockLength = blockLength;
        }
        public MultiBuffer(byte[] firstBuffer): this (firstBuffer.Length)
        {
            blocks.Add(firstBuffer);
            Length = firstBuffer.Length;
        }


        public int Read(long position, in Span<byte> buffer)
        {
            var readLen = (int)Math.Min(buffer.Length, Length - position);
            if (readLen > 0) ReadFromMultiBlock(position, buffer[..readLen]);
            return readLen;
        }

        private void ReadFromMultiBlock(long position, Span<byte> buffer)
        {
            var (block, blockOffset) = StartingPosition(Math.Min(position, Length));
            for (var bytesRead = 0; bytesRead < buffer.Length;)
            {
                bytesRead += CopyBytes(blocks[block].AsSpan(blockOffset), buffer.Slice(bytesRead));
                block++;
                blockOffset = 0;
            }
        }


        public void Write(long position, in ReadOnlySpan<byte> buffer)
        {
            SetLength(Math.Max(Length, position + buffer.Length));
            WriteToMultiBlock(position, buffer);
        }

        private void WriteToMultiBlock(long position, ReadOnlySpan<byte> buffer)
        {
            var (block, blockOffset) = StartingPosition(position);
            for (int bytesWritten = 0; bytesWritten < buffer.Length;)
            {
                bytesWritten += CopyBytes(buffer.Slice(bytesWritten), blocks[block].AsSpan(blockOffset));
                block++;
                blockOffset = 0;
            }
        }

        private int CopyBytes(in ReadOnlySpan<byte> source, in Span<byte> destination)
        {
            var readLen = Math.Min(source.Length, destination.Length);
            source.Slice(0, readLen).CopyTo(destination);
            return readLen;
        }


        private (int BufferNum, int BufferPosition) StartingPosition(long position) =>
            ((int)position / blockLength, (int)position % blockLength);

        private void EnsureSpaceToWrite(long neededPos)
        {
            while (TotalStorage() < neededPos) CreateNewBlock();
        }

        private void CreateNewBlock() => blocks.Add(new byte[blockLength]);

        private long TotalStorage() => blockLength * blocks.Count;
        
        public void CheckValidPosition(long value)
        {
            if (!IsValidPosition(value)) 
                throw new ArgumentException("Invalid stream position: " + value);
        }
        
        private bool IsValidPosition(long value) => value >= 0 && value <= Length;

        public void SetLength(long value)
        {
            EnsureSpaceToWrite(value);
            Length = value;
        }
    }
}