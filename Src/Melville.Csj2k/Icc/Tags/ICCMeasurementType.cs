using System;
using Melville.CSJ2K.Icc;
using Melville.CSJ2K.Icc.Types;

namespace Melville.CSJ2K.Icc.Tags
{
    public class ICCMeasurementType : ICCTag
    {
        new public int type;
        public int reserved;
        public int observer;
        public XYZNumber backing;
        public int geometry;
        public int flare;
        public int illuminant;

        /// <summary> Construct this tag from its constituant parts</summary>
        /// <param name="signature">tag id</param>
        /// <param name="data">array of bytes</param>
        /// <param name="offset">to data in the data array</param>
        /// <param name="length">of data in the data array</param>
        protected internal ICCMeasurementType(int signature, byte[] data, int offset, int length)
            : base(signature, data, offset, offset + 2 * BitReaders.int_size)
        {
            type = BitReaders.getInt(data, offset);
            reserved = BitReaders.getInt(data, offset + BitReaders.int_size);
            observer = BitReaders.getInt(data, offset + BitReaders.int_size);
            backing = BitReaders.getXYZNumber(data, offset + BitReaders.int_size);
            geometry = BitReaders.getInt(data, offset + (BitReaders.int_size*3));
            flare = BitReaders.getInt(data, offset + BitReaders.int_size);
            illuminant = BitReaders.getInt(data, offset + BitReaders.int_size);
        }
    }
}