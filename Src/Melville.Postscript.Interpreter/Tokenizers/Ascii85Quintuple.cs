using System;
using Melville.Postscript.Interpreter.Values.Interfaces;

namespace Melville.Postscript.Interpreter.Tokenizers
{
    internal ref struct Ascii85Quintuple
    {
        private uint data = 0;

        public Ascii85Quintuple()
        {
        }

        public void AddByte(byte b)
        {
            data *= 85u;
            data += (uint)(b - Ascii85Constants.FirstChar);
        }

        public int WriteDecodedBytesPartial(scoped Span<byte> destination, int bytesPushed)
        {
            PadForMissingBytes(bytesPushed);
            var bytesToWrite = bytesPushed - 1;
            RemoveUnwrittenBytes(bytesToWrite);
            return WriteDecodedBytes(destination, bytesToWrite);
        }

        private void PadForMissingBytes(int sourceBytes)
        {
            if (sourceBytes < 2) 
                throw new PostscriptNamedErrorException("Invalid Ascii85 string","syntaxerror");
            for (int i = sourceBytes; i < 5; i++) 
                AddByte(Ascii85Constants.IncompleteGroupPadding);
        }

        private void RemoveUnwrittenBytes(int bytesToWrite)
        {
            for (int i = 3; i >= bytesToWrite; i--)
            {
                data >>= 8;
            }
        }

        public int WriteDecodedBytes(scoped Span<byte> destination, int bytesToWrite)
        {

            for (int i = bytesToWrite -1; i >= 0; i--)
            {
                destination[i] = (byte)data;
                data >>= 8;
            }
            return bytesToWrite;
        }
    }
}