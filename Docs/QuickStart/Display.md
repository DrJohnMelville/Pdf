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
- A byte[]
- A Stream, which must be readable and seekable with a known length.
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
We are using the SkiaSharp binding here so we start by referencing Melville.Pdf.SkiaSharp.  The PdfReader object has 
ReadFrom and ReadFromFile overrides which look very fsamiliar given the list of acceptable sources as well.  PdfReader
will convert any of the legal sources into a DocumentRenderer.  

The RenderWithSkiaObject is a facade that will render a page from a DocumentRenderer to a SkSurface object, or render
to a SkSurface and then save that surface in PMG format.  The Melville.Pdf.Wpf project includes a similar class, 
RenderToDrawingGroup, which renders pages to a variety of WPF image representations or a PNG stream.  The PNG generation 
methods exist primarily to support the integration testing.  I will not be adding support to write to multiple image 
formats.  The code is available and the trivial task of adopting this code to any desired target format is left as an 
exercise for the reader.

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
