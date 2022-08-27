using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Melville.JpegLibrary.Decoder;

namespace Melville.JpegLibrary.Readers;

    internal ref struct JpegBitReader
    {
        private ReadOnlySequence<byte> _data;
        private ReadOnlySpan<byte> _firstSpan;
        private ulong _buffer; // right-justified bit buffer
        private byte _bitsInBuffer;
        private JpegMarker _nextMarker; // _nextMarker==0: No marker is encountered in the stream; otherwise the next marker in the stream after the bits read in the buffer.

        public JpegBitReader(ReadOnlySequence<byte> data)
        {
            _data = data;
            _firstSpan = default;
            _buffer = 0;
            _bitsInBuffer = 0;
            _nextMarker = 0;
        }

        public int RemainingBits => 8 * (int)(_data.Length + _firstSpan.Length) + _bitsInBuffer;

        public void AdvanceAlignByte()
        {
            _bitsInBuffer = (byte)(_bitsInBuffer - (_bitsInBuffer % 8));
            FillBuffer();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryLoadFirstSpan()
        {
            Debug.Assert(_firstSpan.IsEmpty);

            if (_data.IsEmpty)
            {
                return false;
            }

#if NO_READONLYSEQUENCE_FISTSPAN
            _firstSpan = _data.First.Span;
#else
            _firstSpan = _data.FirstSpan;
#endif
            _data = _data.Slice(_firstSpan.Length);

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryReadNextByte(out byte byteRead)
        {
            if (_firstSpan.IsEmpty)
            {
                if (!TryLoadFirstSpan())
                {
                    byteRead = 0;
                    return false;
                }
            }

            Debug.Assert(!_firstSpan.IsEmpty);
            byteRead = _firstSpan[0];
            _firstSpan = _firstSpan.Slice(1);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryPeekNextByte(out byte bytePeeked)
        {
            if (_firstSpan.IsEmpty)
            {
                if (!TryLoadFirstSpan())
                {
                    bytePeeked = 0;
                    return false;
                }
            }

            Debug.Assert(!_firstSpan.IsEmpty);
            bytePeeked = _firstSpan[0];
            return true;
        }

        /// <summary>
        /// Fill the buffer until the buffer contains 32 bits data, or the stream ends, or a marker is encountered.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private int FillBuffer()
        {
            // Read until we have at least 32 bits in the buffer
            while (_bitsInBuffer < 32)
            {
                if (_nextMarker != 0)
                {
                    return _bitsInBuffer;
                }
                // Read next byte
                if (!TryReadNextByte(out byte byteRead))
                {
                    break;
                }
                if (byteRead == 0xff)
                {
                    // A marker may be encountered, lets peek it out to see what it is.
                    if (!TryPeekNextByte(out byteRead))
                    {
                        // The stream ended prematurely
                        break;
                    }
                    if (byteRead == 0xff)
                    {
                        // It is the padding byte, continue reading
                        continue;
                    }
                    // It is not the padding byte, advance past it
                    _ = TryReadNextByte(out _);
                    if (byteRead != 0)
                    {
                        // It is a marker
                        _nextMarker = (JpegMarker)byteRead;
                        break;
                    }
                    // It a stuffed byte
                    byteRead = 0xff;
                }
                // Put the byte read in the buffer
                _buffer = (_buffer << 8) | byteRead;
                _bitsInBuffer += 8;
            }
            return _bitsInBuffer;
        }

        public JpegMarker TryReadMarker()
        {
            if (_bitsInBuffer == 0)
            {
                JpegMarker marker = _nextMarker;
                _nextMarker = 0;
                return marker;
            }
            return 0;
        }

        public JpegMarker TryPeekMarker()
        {
            return _bitsInBuffer == 0 ? _nextMarker : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int PeekBits(int length, out int bitsPeeked)
        {
            int bitsInBuffer = _bitsInBuffer;
            if (bitsInBuffer < length)
            {
                bitsInBuffer = FillBuffer();
                if (bitsInBuffer < length)
                {
                    bitsPeeked = bitsInBuffer;
                    return ((int)_buffer << (length - bitsInBuffer)) & ((1 << length) - 1) | ((1 << (length - bitsInBuffer)) - 1);
                }
            }
            int remainingBits = bitsInBuffer - length;
            bitsPeeked = length;
            return (int)(_buffer >> remainingBits) & ((1 << length) - 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryAdvanceBits(int length, out bool isMarkerEncountered)
        {
            if (_bitsInBuffer < length)
            {
                if (!TryLoadBits(length, out isMarkerEncountered))
                {
                    return false;
                }
            }
            _bitsInBuffer = (byte)(_bitsInBuffer - length);
            isMarkerEncountered = false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryReadBits(int length, out int bits, out bool isMarkerEncountered)
        {
            if (_bitsInBuffer < length)
            {
                if (!TryLoadBits(length, out isMarkerEncountered))
                {
                    bits = default;
                    return false;
                }
            }
            _bitsInBuffer = (byte)(_bitsInBuffer - length);
            bits = (int)(_buffer >> _bitsInBuffer) & ((1 << length) - 1);
            isMarkerEncountered = false;
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool TryLoadBits(int length, out bool isMarkerEncountered)
        {
            int bitsInBuffer = FillBuffer();
            if (bitsInBuffer < length)
            {
                isMarkerEncountered = bitsInBuffer == 0 && _nextMarker != 0;
                return false;
            }
            isMarkerEncountered = false;
            return true;
        }
    }
