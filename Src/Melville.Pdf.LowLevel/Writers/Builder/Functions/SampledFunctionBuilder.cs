using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Melville.Parsing.AwaitConfiguration;
using Melville.Parsing.Streams;
using Melville.Parsing.VariableBitEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Wrappers.Functions;

namespace Melville.Pdf.LowLevel.Writers.Builder.Functions;

/// <summary>
/// This builder allows me to construct sampled functions
/// </summary>
public readonly struct SampledFunctionBuilder
{
    private readonly List<SampledFunctionInput> inputs = new();
    private readonly List<SampledFunctionOutput> outputs = new();
    private readonly int bitsPerSample;
    private readonly SampledFunctionOrder order;
    private readonly ClosedInterval sampleDomain;

    /// <summary>
    /// Create a smapled function builder.
    /// </summary>
    /// <param name="bitsPerSample">The number of bits used to store each sample.</param>
    /// <param name="order">Selects linear or bicubic interpolation.</param>
    public SampledFunctionBuilder(
        int bitsPerSample, SampledFunctionOrder order = SampledFunctionOrder.Linear)
    {
        this.bitsPerSample = bitsPerSample;
        this.order = order;
        VerifyOrder(order);
        VerifySampleBitLength();
        sampleDomain = new ClosedInterval(0, (1 << bitsPerSample) - 1);
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

    /// <summary>
    /// Add an input to the sampled function.
    /// </summary>
    /// <param name="samples">The number of samples to generate in this dimension.</param>
    /// <param name="domain">The domain for the input.</param>
    /// <param name="encode">The encoding interval for the input/</param>
    public void AddInput(int samples, in ClosedInterval domain, in ClosedInterval? encode = null) => 
        inputs.Add(new SampledFunctionInput(domain, encode??(0, samples-1), samples));

    /// <summary>
    /// Add an output to a sampled function with four inputs
    /// </summary>
    /// <param name="defn">A C# function that will be sampled to define the function</param>
    /// <param name="range">The range of the output</param>
    /// <param name="decode">The decode array for the output</param>
    public void AddOutput(
        Func<double, double, double, double, double> defn, 
        ClosedInterval range, ClosedInterval? decode = null) =>
        AddOutput(arg => defn(arg[0], arg[1], arg[2], arg[3]), range, decode);

    /// <summary>
    /// Add an output to a sampled function with three inputs
    /// </summary>
    /// <param name="defn">A C# function that will be sampled to define the function</param>
    /// <param name="range">The range of the output</param>
    /// <param name="decode">The decode array for the output</param>
    public void AddOutput(
        Func<double, double, double, double> defn, ClosedInterval range, ClosedInterval? decode = null) =>
        AddOutput(arg => defn(arg[0], arg[1], arg[2]), range, decode);

    /// <summary>
    /// Add an output to a sampled function with two inputs
    /// </summary>
    /// <param name="defn">A C# function that will be sampled to define the function</param>
    /// <param name="range">The range of the output</param>
    /// <param name="decode">The decode array for the output</param>
    public void AddOutput(
        Func<double, double, double> defn, ClosedInterval range, ClosedInterval? decode = null) =>
        AddOutput(arg => defn(arg[0], arg[1]), range, decode);

    /// <summary>
    /// Add an output to a sampled function with one input
    /// </summary>
    /// <param name="defn">A C# function that will be sampled to define the function</param>
    /// <param name="range">The range of the output</param>
    /// <param name="decode">The decode array for the output</param>
    public void AddOutput(
        Func<double, double> defn, ClosedInterval range, ClosedInterval? decode = null) =>
        AddOutput(arg => defn(arg[0]), range, decode);


    /// <summary>
    /// Add an output to a sampled function with an arbitrary number
    /// </summary>
    /// <param name="defn">A SimpleFunctionResult that will be sampled to define the function</param>
    /// <param name="range">The range of the output</param>
    /// <param name="decode">The decode array for the output</param>
    public void AddOutput(
        SimpleFunctionResult defn, ClosedInterval range, ClosedInterval? decode = null) =>
        AddOutput(new SampledFunctionOutput(range, decode ?? range, defn));

    private void AddOutput(SampledFunctionOutput sfo) => outputs.Add(sfo);

    /// <summary>
    /// Create the PdfStream defining this function.
    /// </summary>
    /// <param name="members">A DictionaryBuilder that should be used to build the stream.</param>
    /// <returns>The stream that defines this function.</returns>
    private DictionaryBuilder DictionaryEntries(in DictionaryBuilder members) =>
        members
            .WithItem(KnownNames.FunctionType, 0)
            .WithItem(KnownNames.Domain, inputs.Select(i => i.Domain).AsPdfArray(inputs.Count))
            .WithItem(KnownNames.Range, outputs.Select(i => i.Range).AsPdfArray(outputs.Count))
            .WithItem(KnownNames.Size, SizeArray())
            .WithItem(KnownNames.BitsPerSample, bitsPerSample)
            .WithItem(KnownNames.Order, OrderIfNotLinear())
            .WithItem(KnownNames.Encode, EncodeArray())
            .WithItem(KnownNames.Decode, DecodeArray());

    private PdfArray SizeArray() => new(inputs.Select(i => (PdfIndirectObject)i.Samples).ToArray());

    private PdfDirectObject OrderIfNotLinear() => 
        order == SampledFunctionOrder.Linear?PdfDirectObject.CreateNull(): 3;

    private PdfDirectObject EncodeArray() =>
        EncodeArrayIsTrivial()
            ? PdfDirectObject.CreateNull()
            : inputs.Select(i => i.Encode).AsPdfArray(inputs.Count);

    private bool EncodeArrayIsTrivial() => inputs.All(i => i.EncodeTrivial());
        
    private PdfDirectObject DecodeArray() =>
        DecodeArrayIsTrivial()
            ? PdfDirectObject.CreateNull()
            : outputs.Select(i => i.Decode).AsPdfArray(outputs.Count);

    private bool DecodeArrayIsTrivial()
    {
        return outputs.All(i => i.DecodeTrivial());
    }

    private async ValueTask<MultiBufferStream> SamplesStreamAsync()
    {
        var ret = new MultiBufferStream();
        var bitWriter = new BitStreamWriter(ret, bitsPerSample);
        WriteSamplesToWriter(bitWriter);
        await bitWriter.FinishAsync().CA();
        return ret;
    }

    private void WriteSamplesToWriter(in BitStreamWriter bitWriter) => 
        RecursiveWriteSample(bitWriter, 
            stackalloc int[inputs.Count], stackalloc double[inputs.Count], inputs.Count - 1);

    private void RecursiveWriteSample(
        in BitStreamWriter bitStreamWriter, in Span<int> indices, in Span<double> values, int index)
    {
        if (index < 0)
        {
            WriteOutputs(bitStreamWriter, values);
            return;
        }

        for (int i = 0; i < inputs[index].Samples; i++)
        {
            indices[index] = i;
            values[index] = inputs[index].InputAtSampleLocation(i);
            RecursiveWriteSample(bitStreamWriter, indices, values, index - 1);
        }
    }

    private void WriteOutputs(in BitStreamWriter bitStreamWriter, in Span<double> values)
    {
        foreach (var singleOutput in outputs)
        {
            WriteSingleOutput(bitStreamWriter, values, singleOutput);
        }
    }

    private void WriteSingleOutput(
        in BitStreamWriter bitStreamWriter, in Span<double> value, SampledFunctionOutput singleOutput)
    {
        var unencodedValue = singleOutput.Definition(value);
        var encodedValue = (uint)singleOutput.Decode.MapTo(sampleDomain, unencodedValue);
        bitStreamWriter.Write(encodedValue);
    }
    
    /// <summary>
    /// Build the resulting function.
    /// </summary>
    /// <returns>The resulting function, as a PDF stream</returns>
    public ValueTask<PdfStream> CreateSampledFunctionAsync() =>
        CreateSampledFunctionAsync(new DictionaryBuilder());

    /// <summary>
    /// Build the resulting function.
    /// </summary>
    /// <param name="members">The DictionaryBuilder which should be used to build the function</param>
    /// <returns>The resulting function, as a PDF stream</returns>
    public async ValueTask<PdfStream> CreateSampledFunctionAsync(DictionaryBuilder members) =>
        DictionaryEntries(members).AsStream(await SamplesStreamAsync().CA());
}