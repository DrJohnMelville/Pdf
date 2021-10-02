﻿using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing
{
    public class StaticSingleFilter: IApplySingleFilter
    {
        public async ValueTask<Stream> Encode(Stream source, PdfObject filter, PdfObject parameter) =>
            await 
                StaticCodecFactory.CodecFor((PdfName)await filter.DirectValueAsync())
                    .EncodeOnReadStream(source, parameter);

        public async ValueTask<Stream> Decode(Stream source, PdfObject filter, PdfObject parameter) =>
            await 
                StaticCodecFactory.CodecFor((PdfName)await filter.DirectValueAsync())
                    .DecodeOnReadStream(source, parameter);
    }
}