using System;
using System.Text;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.LowLevel.Model.Primitives;

namespace Melville.Pdf.LowLevel.Model.Conventions;

/// <summary>
/// This is the directory of all the PdfName flyweights in the system.
/// </summary>
public static class NameDirectory
{
    private static readonly NameDictionay allKnownNames = new ();

    /// <summary>
    /// Add a PdfName that is known to not be in the dictionary
    /// </summary>
    /// <param name="bytes">A bybte span with the name to be added</param>
    /// <returns>The item parameter</returns>
    public static PdfName ForceAdd(in ReadOnlySpan<byte> bytes)
    {
        var item = PdfNameFactory.Create(bytes);
        allKnownNames.ForceAdd(item.Bytes, item);
        return item;
    }
    
    /// <summary>
    /// Get a PdfName matching a given span.
    /// </summary>
    /// <param name="nameText">Span containing the name of the PdfName to get</param>
    /// <returns>A flyweight PdfName corresponding to that name</returns>
    public static PdfName Get(ReadOnlySpan<byte> nameText)
    {
        lock (allKnownNames)
        {
            return allKnownNames.GetOrCreate(nameText);
        }
    }

    /// <summary>
    /// Get a PdfName corresponding to a given name.
    /// </summary>
    /// <param name="nameText"></param>
    /// <returns></returns>
    public static PdfName Get(string nameText)
    {
        Span<byte> span = stackalloc byte[Encoding.UTF8.GetMaxByteCount(nameText.Length)];
        var length = Encoding.UTF8.GetBytes(nameText, span);
        return Get(span[..length]);
    }
        
}

/// <summary>
/// A directory of hard coded well known PdfNames
/// </summary>
public static partial class KnownNames
{
}