﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Filters.Ascii85Filter;
using Melville.Pdf.LowLevel.Filters.AsciiHexFilters;
using Melville.Pdf.LowLevel.Filters.FlateFilters;
using Melville.Pdf.LowLevel.Filters.LzwFilter;
using Melville.Pdf.LowLevel.Filters.RunLengthEncodeFilters;
using Melville.Pdf.LowLevel.Filters.StreamFilters;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters
{
    public interface ICodecDefinition
    {
        public ValueTask<Stream>  EncodeOnReadStream(Stream data, PdfObject? parameters);
        public ValueTask<Stream>  EncodeOnWriteStream(Stream data, PdfObject? parameters);
        ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters);

    }

    public class CodecDefinition: ICodecDefinition
    {
        private readonly Func<PdfObject?, ValueTask<IStreamFilterDefinition>> encoder;
        private readonly Func<PdfObject?, ValueTask<IStreamFilterDefinition>> decoder;

        public CodecDefinition(
            Func<PdfObject?, ValueTask<IStreamFilterDefinition>> encoder, 
            Func<PdfObject?, ValueTask<IStreamFilterDefinition>> decoder)
        {
            this.encoder = encoder;
            this.decoder = decoder;
        }

        public async ValueTask<Stream> EncodeOnReadStream(Stream data, PdfObject? parameters) =>
            ReadingFilterStream.Wrap(data, await encoder(parameters));

        public async ValueTask<Stream> DecodeOnReadStream(Stream input, PdfObject parameters) =>
            ReadingFilterStream.Wrap(input, await decoder(parameters));

        public async ValueTask<Stream> EncodeOnWriteStream(Stream data, PdfObject? parameters) => 
            new WritingFilterStream(data, await encoder(parameters));
    }

    public static class CodecFactory
    {
        public static ICodecDefinition CodecFor(PdfName name) => codecs[name];
        
        private static Dictionary<PdfName, ICodecDefinition> codecs = CreateDictionary();
        private static Dictionary<PdfName, ICodecDefinition> CreateDictionary()
        {
            Dictionary<PdfName, ICodecDefinition> ret = new();
            ret.Add(KnownNames.ASCIIHexDecode, 
                ConstantCodec(new AsciiHexEncoder(), new AsciiHexDecoder()));
            ret.Add(KnownNames.ASCII85Decode, 
                ConstantCodec(new Ascii85Encoder(), new Ascii85Decoder()));
            ret.Add(KnownNames.RunLengthDecode, 
                ConstantCodec(new RunLengthEncoder(), new RunLengthDecoder()));
            ret.Add(KnownNames.LZWDecode, new CodecDefinition(
                    async p => new LzwEncodeFilter(await p.EarlySwitchLength()),
                    async p => new LzwDecodeFilter(await p.EarlySwitchLength())));
            ret.Add(KnownNames.FlateDecode, new FlateCodecDefinition());
            ret.Add(KnownNames.Fl, new FlateCodecDefinition());
            return ret;
        }

        private static CodecDefinition ConstantCodec(
            IStreamFilterDefinition encoder, IStreamFilterDefinition decoder) =>
            new(_=>new ValueTask<IStreamFilterDefinition>(encoder), 
                _=>new ValueTask<IStreamFilterDefinition>(decoder));
    }
}