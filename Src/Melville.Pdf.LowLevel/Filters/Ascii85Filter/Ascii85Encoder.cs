using System;
using System.Text;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.Ascii85Filter
{
    public class Ascii85Encoder:IEncoder
    {
        // MIT Licensed code borrowed from
        // https://github.com/LogosBible/Logos.Utility/blob/master/src/Logos.Utility/Ascii85.cs
        const byte firstChar = (byte)'!';

        public byte[] Encode(byte[] data, PdfObject? parameters)
        {
            // preallocate a StringBuilder with enough room to store the encoded bytes
            var ret = new OutputBuffer(new byte[(data.Length * 5 / 4)+5]);

            // walk the bytes
            int count = 0;
            uint value = 0;
            foreach (byte b in data)
            {
                // build a 32-bit value from the bytes
                value <<= 8;
                value |= b;
                count++;

                // every 32 bits, convert the previous 4 bytes into 5 Ascii85 characters
                if (count == 4)
                {
                    EncodeCompleteGroup(value, ref ret);
                    count = 0;
                    value = 0;
                }
            }

            // encode any remaining bytes (that weren't a multiple of 4)
            if (count > 0)
                EncodeValue(ref ret, value << (8*(4-count)), count + 1);

            return ret.Result();
        }

        private static void EncodeCompleteGroup(uint value, ref OutputBuffer ret)
        {
            if (value == 0)
                ret.Append('z');
            else
                EncodeValue(ref ret, value, 5);
        }

        private static void EncodeValue(ref OutputBuffer buffer, uint value, int paddingBytes)
        {
            for (int index = 4; index >= 0; index--)
            {
                if (index < paddingBytes) buffer.Set((value % 85) + firstChar, index);
                value /= 85;
            }

            buffer.Increment(paddingBytes);
        }
    }
}