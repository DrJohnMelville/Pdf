using System;

namespace Melville.Pdf.LowLevel.Filters.LzwFilter
{
    public class EncoderDictionary
    {
        // PDF specifies maximum bit length of 12 so maximum code is 4095
        private readonly Entry[] entries = new Entry[LzwConstants.MaxTableSize];
        private int nextEntry = LzwConstants.EndOfFileCode + 1;
        
        public EncoderDictionary()
        {
            for (int i = 0; i < 256; i++)
            {
                entries[i].Element = (byte) i;
            }
        }

        private struct Entry
        {
            public byte Element;
            public short FirstChild;
            public short NextSibling;
        }
        
        public bool GetOrCreateNode(short rootIndex, byte nextByte, out short outputItem)
        {
            return GetOrCreateNode(ref entries[rootIndex], nextByte, out outputItem);
        }

        private bool GetOrCreateNode(ref Entry entry, byte nextByte, out short outputItem)
        {
            if (entry.FirstChild == 0)
            {
                outputItem = CreateItem(nextByte);
                entry.FirstChild = outputItem;
                return false;
            }

            return SearchLinkedList(ref entries[entry.FirstChild], entry.FirstChild, nextByte, out outputItem);
        }

        private bool SearchLinkedList(ref Entry entry, short thisIndex, byte nextByte, out short outputItem)
        {
            if (nextByte == entry.Element)
            {
                outputItem = thisIndex;
                return true;
            }

            if (entry.NextSibling == 0)
            {
                entry.NextSibling = outputItem = CreateItem(nextByte);
                return false;
            }

            return SearchLinkedList(ref entries[entry.NextSibling], entry.NextSibling,
                nextByte, out outputItem);
        }

        
        private short CreateItem(byte nextByte)
        {
            var ret = (short)nextEntry++;
            entries[ret].Element = nextByte;
            return ret;
        }
    }
}