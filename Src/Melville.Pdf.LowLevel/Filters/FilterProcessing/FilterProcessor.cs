using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.Objects;

namespace Melville.Pdf.LowLevel.Filters.FilterProcessing
{
    public enum StreamFormat
    {
        DiskRepresentation = int.MinValue,
        WithoutImplicitEncryption = -1,
        PlainText = int.MaxValue
    }
    public interface IFilterProcessor
    {
        ValueTask<Stream> StreamInDesiredEncoding(
            Stream src, StreamFormat sourceFormat, StreamFormat desiredEncoding);
    }
    public class FilterProcessor: IFilterProcessor
    {

        private readonly IReadOnlyList<PdfObject> filters;
        private readonly IReadOnlyList<PdfObject> parameters;
        private readonly IApplySingleFilter singleFilter;

        public FilterProcessor(
            IReadOnlyList<PdfObject> filters, 
            IReadOnlyList<PdfObject> parameters, IApplySingleFilter singleFilter)
        {
            this.filters = filters;
            this.parameters = parameters;
            this.singleFilter = singleFilter;
        }

        public async ValueTask<Stream> StreamInDesiredEncoding(Stream src, StreamFormat sourceFormat,
            StreamFormat desiredEncoding)
        {
            return sourceFormat.CompareTo(desiredEncoding) switch
            {
                < 0 => await Decode(src, sourceFormat, desiredEncoding),
                0 => src,
                _ => await Encode(src, sourceFormat, desiredEncoding)
            };
        }

        private async ValueTask<Stream> Encode(Stream source, StreamFormat sourceFormat, StreamFormat targetFormat)
        {
            var inclusiveUpperBound = Math.Min(filters.Count-1, (int)sourceFormat);
            var exclusiveLowerBound = Math.Max((int)targetFormat, -1);
            var ret = source;
            for (int i = inclusiveUpperBound; i < exclusiveLowerBound; i--)
            {
                ret = await singleFilter.Encode(ret, filters[i], TryGetParameter(i));
            }

            return ret;
        }
        private async ValueTask<Stream> Decode(Stream source, StreamFormat sourceFormat, StreamFormat targetFormat)
        {
            var firstFilter = Math.Max(0, (int)sourceFormat+1);
            var oneMoreThanLastFilter = Math.Min((int)targetFormat, filters.Count);
            var ret = source;
            for (int i = firstFilter; i < oneMoreThanLastFilter; i++)
            {
                ret = await singleFilter.Decode(ret, filters[i], TryGetParameter(i));
            }

            return ret;
        }

        private PdfObject TryGetParameter(int i) => i < parameters.Count?parameters[i]:PdfTokenValues.Null;
    }
}