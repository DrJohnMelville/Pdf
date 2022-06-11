using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Melville.Linq;
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
                #warning -- need to use the operation here.
                target[i, j] = bit == 1;
            }
        }

        UdpConsole.WriteLine(StringJoiner.JoinLines(reference.BitmapString(), target.BitmapString(), "|||"));
    }
}

public static class StringJoiner
{
    public static string JoinLines(string a, string b, string divider)
    {
        var listA = SplitLines(a);
        var listB = SplitLines(b);
        return (listA.Length - listB.Length) switch
        {
            < 0 => JoinLines(Pad(listA, StringOfLength(listA[0].Length)), listB, divider),
            0 => JoinLines(listA, listB, divider),
            > 0 => JoinLines(listA, Pad(listB, StringOfLength(listB[0].Length)), divider)
        };
    }

    private static string JoinLines(IEnumerable<string> listA, IEnumerable<string> listB, string divider) => 
        string.Join("\r\n", listA.Zip(listB, (a, b) => $"{a}{divider}{b}"));

    private static IEnumerable<string> Pad(string[] listA, string extra)
    {
        foreach (var item in listA) yield return item;
        while (true) yield return extra;
    }

    private static string StringOfLength(int length)
    {
        return string.Join("", Enumerable.Repeat(" ", length));
    }

    private static string[] SplitLines(string a)
    {
        return a.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
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
