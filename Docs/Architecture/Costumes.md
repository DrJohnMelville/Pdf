# Costume Pattern

This is a pattern I designed to compensate for PDF's extremely weak "type system."  Pdf files are composed of 9 types:
null, boolean, numeric, string, name, array, dictionary or stream.  PDF documents, however are composed of things like
pages, page trees, fonts, images, patterns, and so forth.  90% of these PDF concepts are represented in the file as either
dictionaries or streams.  The costume pattern is an extremely lightweight mechanism to give these PDF concepts a 
corresponding construct in the code.

For example, let's look at the PdfFont costume type.

````c#
public readonly struct PdfFont
{
    public PdfFont(PdfDictionary lowLevel)
    {
        LowLevel = lowLevel;
    }

    public readonly PdfDictionary LowLevel { get; }

    public async ValueTask<PdfName> SubTypeAsync() => 
        await LowLevel.GetOrDefaultAsync(KnownNames.Subtype, 
            await LowLevel.GetOrDefaultAsync(KnownNames.S, KnownNames.Type1).CA()).CA();
    
    public ValueTask<PdfObject?> EncodingAsync() => LowLevel.GetOrNullAsync<PdfObject>(KnownNames.Encoding);

    public ValueTask<PdfDictionary?> DescriptorAsync() =>
        LowLevel.GetOrNullAsync<PdfDictionary>(KnownNames.FontDescriptor);

    public async ValueTask<FontFlags> FontFlagsAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            ? (FontFlags)(await descriptor.GetOrDefaultAsync(KnownNames.Flags, 0).CA())
            : FontFlags.None;

    public async ValueTask<PdfStream?> EmbeddedStreamAsync() =>
        await DescriptorAsync().CA() is { } descriptor
            && (
            descriptor.TryGetValue(KnownNames.FontFile, out var retTask) ||
                descriptor.TryGetValue(KnownNames.FontFile2, out retTask) ||
              descriptor.TryGetValue(KnownNames.FontFile3, out retTask) )? 
                await retTask.CA() as PdfStream : null;
                
          // ... the actual PdfFont has more methods but you get the idea
}
````
Notice a few important features of the costume pattern.
- The PdfFont type is a readonly struct.
- It has a single field, which happens to be a PDF dictionary.
- Operations on the costume abstract Font concepts and hide their representation in the dictionary.

Costume types are a very thin veneer over the original type.  Structs are passed by value, but this is OK because copying
a PdfFont is exactly the same as passing the underlying PdfDictionary by reference -- a single reference is copied in 
either case.  Constructing a PdfFont is similarly simple and actually gets optimized away to nothing by the inliner.

Fonts however do not have keys and values.  A font does have a Encoding property.  In the disk representation that can be 
noted as a PdfObject value with key of Encoding, or null, or no Encoding key at all.  The PdfFont costume encapsulates all
this detail by returning a PdfEncoding costume, which can wrap any of those three options.

The PdfFont costume encapsulates a number of common pdf data model idioms.
- Default Values, see FontFlagsAsync
- Alternate names for the same value, see SubTypeAsync or EmbeddedStreamAsync
- The PdfFont simplifies the PDF data model, by hiding the FontDescriptor completely.

This costume pattern is quite common in the codebase.  For costumes that are readonly structs it is essential not to
add fields to the object.  There are a few wrapper types used like costumes, but are actually classes.  PdfPages and 
TilePatterns are objects because there is an inheritance hierarchy of objects that have a content stream.  Pdf functions
are classes because there is expensive parsing before a function can be evaluated and it makes sense to cache the parsed
function representations.  Most of the other high level pdf objects are costume types.