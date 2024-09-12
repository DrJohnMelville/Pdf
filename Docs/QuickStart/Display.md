# How to Render Pdf

Melville.Pdf is designed to be a flexible PDF engine applicable in many 
contexts, both on the desktop and on the server.  Often you will interact
with various "bindings" to render PDF content in various formats.  At the
current time I have implemented 2 bindings: WPF and SkiaSharp.

## 1. The Wpf Way -- Show PDF in a Control
````xml
<UserControl x:Class="Melville.Pdf.WpfViewer.Home.HomeView"
             ...
             xmlns:controls="clr-namespace:Melville.Pdf.Wpf.Controls;assembly=Melville.Pdf.Wpf"
              >
    <controls:PdfViewer Source="C:\File.pdf"/>
</UserControl>
````
The WPF PdfViewer control lives in Melville.Pdf.Wpf.  The Pdf control is quite simple right now, but it may
grow in the future.  The Source property on the PdfViewer is of type object and it can take any of the
following types:
- A string, which is interpreted as the path of the PDF file on the file system.
- A byte[ ] array.
- A Stream, which must be readable and seekable with a known length.
- An IMultiplexSource which is a Melville.Pdf abstraction for a sequence of bytes that can have multiple overlapping readers.
- A PdfLowLevelDocument which presents the PDF file as a graph of Pdf objects.
- A PdfDocument, which is a thin ["Costume Type"](../Architecture/Costumes.md) around PdfLowLevelDocument
  that presents the PDF file as a sequence of pages.
- A DocumentRenderer which represents a PdfDocument of which some pages may have been previously rendered.
  The DocumentRenderer manages caching and the visibility of optional content.

## 2. Using Skia Sharp -- save PDF page to a png file
````c#
using System.IO;
using System.Threading.Tasks;
using Melville.Pdf.Model;
using Melville.Pdf.SkiaSharp; // Need to reference project Melville.Pdf.SkiaSharp

namespace Test;

var document = await new PdfReader().ReadFromFile("File.Pdf");
var output = new MemoryStream();
await RenderWithSkia.ToPngStreamAsync(document, 1, output);
````
We are using the SkiaSharp binding here so we start by referencing Melville.Pdf.SkiaSharp.  The PdfReader object has ReadFrom and ReadFromFile overrides which look very fsamiliar given the list of acceptable sources as well.  PdfReader will convert any of the legal sources into a DocumentRenderer.  

The RenderWithSkiaObject is a facade that will render a page from a DocumentRenderer to a SkSurface object, or render to a SkSurface and then save that surface in PMG format.  The Melville.Pdf.Wpf project includes a similar class, RenderToDrawingGroup, which renders pages to a variety of WPF image representations or a PNG stream.  The PNG generation methods exist primarily to support the integration testing.  I will not be adding support to write to multiple image formats.  The code is available and the trivial task of adopting this code to any desired target format is left as an exercise for the reader.

The DocumentRenderer class is thread safe.  It is just fine to call DocumentRenderer.RenderPageTo() simultaneously on different threads, or have overlapping calls on the same thread.  The two threads might both need to access the same source stream if they access require PDF objects that have not been cached yet.  Melville.PDF will negotiate consistent access between the various threads.    

## 3. Opening a password protected PDF
Users provide passwords to Melville.Pdf by implementing the IPasswordType interface, which itself declares a single method
````c#
public interface IPasswordSource
{
    ValueTask<(string?, PasswordType)> GetPasswordAsync();
}
````
The method returns a tuple containing a string password and an enum specifying the password type.
Returning a null string in the first position of the tuple indicates that the user has cancelled
the open operation.  Otherwise, the library will call GetPasswordAsync repeatedly until a working or
null password is returned.  Example implementations of this interface in the library include ConstantPasswordSource,
PasswordBox, and PasswordQuery.

Once you have an IPasswordSource it is just a matter or providing it, directly or indirectly, to the PdfReader.  You can
supply it as a property of the PdfViewer:
````xml
    <controls:PdfViewer Source="C:\File.pdf" PasswordSource = "{Binding MyPasswordSource}"/>
````
or as a constructor parameter to the PdfReader.
````c#
     var document = await new PdfReader(myPasswordSource).ReadFromFile("File.Pdf");
````

## 4. Using your own named font mapper.
Most of the fonts used in most PDF documents are embedded directly in the document.  PDF, however, defines 14 standard fonts
which must be supplied by conforming readers.  Furthermore, PDF allows non-embedded fonts which must be mapped to system
fonts, either by name, or using a FontFlags enum to pick an acceptable replacement font.

Default font mapping can be specified using an implementation of IDefaultFontMapper.  IDefaultFontMapper is defined as:

````c#
public interface IDefaultFontMapper
{
    ValueTask<IRealizedFont> FontFromName(PdfName font, FontFlags flags, FreeTypeFontFactory factory);
}
````
The FontFromName method attempt first to find a font matching the name.  Failing that it selects a font based on
the FontFlags enum.  The method must return a correct font for the 14 standard PDF Font names which are:
- Courier
- CourierBold
- CourierOblique
- CourierBoldOblique
- Helvetica
- HelveticaBold
- HelveticaOblique
- HelveticaBoldOblique
- Times
- TimesBold
- TimesOblique
- TimesBoldOblique
- Symbol
- ZapfDingbats

Melville.PDF supplies two implementations of IDefaultFontMapper.
- WindowsDefaultFontMapper loads fonts from the GlobalTypeResources.SystemFontLibrary.  The default fonts
are mapped to core fonts that have shipped with every version of Windows for over a decade.  Other font
names are loaded from the system font library if a font with the correct name exists, otherwise a default
font is substituted.
- The SelfContainedDefaultFonts class is in Melville.Pdf.FontLibrary keeps font rendering completely 
independent of the SystemFontLibrary.  Default fonts are mapped to open source font programs contained
within the Melville.Pdf.FontLibrary assembly.  This increases the size of the program, and will not use
fonts referenced but not embedded in the PDF that fortuitously happen to be found in the system font library.
This class however provides predictable rendering regardless of fonts which may have been removed from the
system font library.

Once you have an IDefaultFontMapper just pass it in to the PdfReader constructor.
````c#
     var document = await new PdfReader(myFontMapper).ReadFromFile("File.Pdf");
````
The WPF PdfViewer control does not have a convenience method to use custom font mapping.  If you are using WPF then you
are on windows and the default windows mapper is highly likely to serve you well.  If needed you could use your own
PdfReader to read the file into a DocumentRenderer and then bind the Source property on the PdfViewer.


## 4. Extract all images from a file.
The ImageExtractor functions as a rendering target.  Rather than painting the page, it stores the images.

````
public async Task ExtractImages()
{
    int index = 1;
    var doc = await new PdfReader().ReadFromFile(<FileName>);
    await foreach (var image in doc.CollapsedImagesFromAsync())
    {
        var skBitmap = await image.ToSkBitmapAsync();
        await using var output = File.Create(@"<BasePath>" + $"\\{index++}.jpg");
        skBitmap.Encode(SKEncodedImageFormat.Jpeg, 90).SaveTo(output);
    }
}
````

Notice that this sample depends on BOTH the Melville.Pdf.SkiaSharp and 
Melville.Pdf.ImageExtractor assemblies.  Melville.PDF does not have a native PNG or
JPEG encoder.  Melville.Pdf.ImageExtractor extracts images to an IExtractedBitmap
that extends IPdfBitmap, and contains the decoded pixel data.  The sample converts this
to a Skia SkBitmap using a method from Melville.Pdf.SkiaSharp and then uses Skia to encode
the image file.  Similar capabilities exist is Melville.Pdf to convert to WPF BitmapImpageSource
objects.

Te CollapseImagesFromAsync method will try to find immediately adjacent images and merge
them.  Many PDFs separate images into bands.  This method stitches those back together.
The ImagesFromAsync method does not stitch images together.  Each method takes an optional
additional parameter to extract the images appearing on a specific page.

## 5. Extract text from a file
The TextExtractor functions as a rendering target.  Rather than painting the page, it records, and
reassembles the text written to the page during the rendering process.  The following code will get the
text from the first page of a PDF document.

````
public async Task ExtractImages()
{
    int index = 1;
    var doc = await new PdfReader().ReadFromFile(<FileName>);
    Console.Writeline(await doc.PageTextAsync(1));
}
````

You can also look at the ConcatenatingTextTarget which implements IExtractedTextTarget.  If you
IExtractedTextTarget receivesc callbacks about the render matrix for each character written to the
pdf page.  You can implement IExtractedTextTarget if you need to process the location of the text on
the page.

## 6. Read and write a PDF form.
Unlike the image and text extractors mentioned above the form reader is not a renderer, if knows nothing 
about content streams or even document level pdf concepts like pages or indexes.  The form reader uses the
low-level document and accesses the dictionaries and streams that define PDF forms.  If you modify the values
the form can create a modified low level document.  The modified document can be written using the WriteToWithXRefStreamAsync extension 
method in Melville.Pdf.LowLevel.Writers.DocumentWriters.LowLevelDocumentWriterOperations.

````
public async Task ReadAndWriteForm()
{
    var form = await FormReaderFacade.ReadFormFromFileAsync(<FileName>);
      // modify the form object as desired.
    await  (await form.CreateModifiedDocumentAsync()).WriteToWithXRefStreamAsync(<output stream>);
}
````