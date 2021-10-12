using System;
using System.Collections.Generic;
using System.Linq;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.LowLevel.Writers.Builder
{
    public enum SampledFunctionOrder
    {
        Linear = 1,
        Cubic = 3
    }
    public class SampledFunctionBuilder
    {
        private List<SampledFunctionInput> inputs = new();
        private List<SampledFunctionOutput> outputs = new();
        private int bitsPerSample;
        private readonly SampledFunctionOrder order;

        public SampledFunctionBuilder(
            int bitsPerSample, SampledFunctionOrder order = SampledFunctionOrder.Linear)
        {
            this.bitsPerSample = bitsPerSample;
            this.order = order;
            VerifyOrder(order);
            VerifySampleBitLength();
        }

        private static void VerifyOrder(SampledFunctionOrder order)
        {
            if (!Enum.IsDefined(order)) throw new ArgumentException("Undefined interpolationType");
        }

        private void VerifySampleBitLength()
        {
            if (bitsPerSample is not (1 or 2 or 4 or 8 or 12 or 16 or 24 or 32))
                throw new ArgumentException("Invalid sample bit length");
        }

        public void AddInput(int samples, in ClosedInterval domain, in ClosedInterval? encode = null) => 
            AddInput(new SampledFunctionInput(domain, encode??(0, samples-1), samples));
        public void AddInput(SampledFunctionInput sfi) => inputs.Add(sfi);

        public void AddOutput(
            Func<double, double, double, double, double> defn, 
              ClosedInterval range, ClosedInterval? decode = null) =>
            AddOutput(arg => defn(arg[0], arg[1], arg[2], arg[3]), range, decode);
        public void AddOutput(
            Func<double, double, double, double> defn, ClosedInterval range, ClosedInterval? decode = null) =>
            AddOutput(arg => defn(arg[0], arg[1], arg[2]), range, decode);
        public void AddOutput(
            Func<double, double, double> defn, ClosedInterval range, ClosedInterval? decode = null) =>
            AddOutput(arg => defn(arg[0], arg[1]), range, decode);
        public void AddOutput(
            Func<double, double> defn, ClosedInterval range, ClosedInterval? decode = null) =>
            AddOutput(arg => defn(arg[0]), range, decode);
        public void AddOutput(
            SimpleFunctionResult defn, ClosedInterval range, ClosedInterval? decode = null) =>
            AddOutput(new SampledFunctionOutput(range, decode ?? range, defn));
        public void AddOutput(SampledFunctionOutput sfo) => outputs.Add(sfo);


        public IEnumerable<(PdfName, PdfObject)> DictionaryEntries()
        {
            return new (PdfName, PdfObject)[]
            {
                (KnownNames.FunctionType, new PdfInteger(0)),
                (KnownNames.Domain, inputs.Select(i=>i.Domain).AsPdfArray(inputs.Count)),
                (KnownNames.Range, outputs.Select(i=>i.Range).AsPdfArray(outputs.Count)),
                (KnownNames.Size, SizeArray()),
                (KnownNames.BitsPerSample, new PdfInteger(bitsPerSample)),
                (KnownNames.Order, OrderIfNotLinear()),
                (KnownNames.Encode, EncodeArray()),
                (KnownNames.Decode, DecodeArray()),
            };
        }

        private PdfArray SizeArray() => new(inputs.Select(i=>new PdfInteger(i.Sammples)).ToList());

        private PdfObject OrderIfNotLinear() => 
            order == SampledFunctionOrder.Linear?PdfTokenValues.Null:new PdfInteger(3);

        private PdfObject EncodeArray() =>
            EncodeArrayIsTrivial()
                ? PdfTokenValues.Null
                : inputs.Select(i => i.Encode).AsPdfArray(inputs.Count);

        private bool EncodeArrayIsTrivial() => inputs.All(i => i.EncodeTrivial());
        
        private PdfObject DecodeArray() =>
            DecodeArrayIsTrivial()
                ? PdfTokenValues.Null
                : outputs.Select(i => i.Decode).AsPdfArray(outputs.Count);

        private bool DecodeArrayIsTrivial() => outputs.All(i => i.DecodeTrivial());
    }

    public static class SampleFunctioBuilderOperations
    {
        public static PdfStream CreateSampledFunction(
            this ILowLevelDocumentBuilder dBuilder, SampledFunctionBuilder fBuilder,
            params (PdfName, PdfObject)[] members)
        {
            return dBuilder.NewStream("StreamData", members.Concat(fBuilder.DictionaryEntries()));
        }
    }
}