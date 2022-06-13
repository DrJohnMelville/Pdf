using System;
using System.Buffers;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.ArithmeticEncodings;
using Melville.Pdf.LowLevel.Filters.Jbig2Filter.BinaryBitmaps;

namespace Melville.Pdf.LowLevel.Filters.Jbig2Filter.GenericRegionRefinements;

public readonly struct GenericRegionRefinementAlgorithm
{
    private readonly IBinaryBitmap target;
    private readonly IBinaryBitmap reference;
    private readonly bool useTypicalPredicition;
    private readonly RefinementTemplateSet template;
    private readonly MQDecoder decoder;

    public GenericRegionRefinementAlgorithm(
        IBinaryBitmap target, IBinaryBitmap reference, 
        bool useTypicalPredicition, in RefinementTemplateSet template, MQDecoder decoder)
    {
        this.target = target;
        this.reference = reference;
        this.useTypicalPredicition = useTypicalPredicition;
        this.template = template;
        this.decoder = decoder;
    }

    public void Read(ref SequenceReader<byte> source)
    {
        if (useTypicalPredicition)
            throw new NotImplementedException("Typical Prediction is not implemented");
        for (int i = 0; i < target.Height; i++)
        {
            for (int j = 0; j < target.Width; j++)
            { 
                ref var context = ref 
                    template.ContextFor(reference, target, i, j);
                var bit = decoder.GetBit(ref source, ref context);
                target[i, j] = bit == 1;
            }
        }
    }
}


public static class UdpConsole
{
    private static UdpClient? client = null;
    private static UdpClient Client
    {
        get
        {
            client ??= new UdpClient();
            return client ;
        }
    }

    public static string WriteLine(string str)
    {
        var bytes = Encoding.UTF8.GetBytes(str);
        Client.Send(bytes, bytes.Length, "127.0.0.1", 15321);
        return str;
    }
}