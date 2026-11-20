using Melville.StreamInterfaces;
using Melville.StreamInterfaces.ParsingBuffers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Melville.Pdf.DataModelTests;

public static class ReaderTextExtensions
{
    extension(IAsyncReader reader) 
    {
        public async ValueTask<string> ReadAsStringAsync()
        {
            using var buf = new ParsingBuffer(reader);
            while (!buf.DoneReadingSource) await buf.ReadBytesAsync();
            return Encoding.UTF8.GetString(buf.Peek());
        }
    }
}
