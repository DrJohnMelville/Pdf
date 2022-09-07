# Introducting Melville.PDF

The purpose of Melville.PDF is simple:  make PDF documents show up in .NET apps __for free__.
Free means free! Free as in speech?  Free as in beer? Who cares?  Free does not mean:
- There is an old version for free but the modern version is locked behind a paywall.
- The version is free if you agree to give your software away.
- It's free but it only works on Windows, or only on the desktop,
- The free version works for short files, or watermarks your files or anything else.
- The free lite version is so feature strapped that you will eventually have to buy the Pro version.

In short free means free.

I have recently become a fan of [Bob Martin](https://blog.cleancoder.com/) and his dogma surrounding
[Clean Code](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882).  You will
have to decide for yourself if my code is clean.  Jumping into any codebase is difficult.  I added some 
introductory material under [Architecture](Docs/Architecture/Overview.md) to  orient you to the codebase.

# Enough Gabbing -- Show Me Some Code!

1. The Wpf Way -- Show PDF in a Control
````xaml
<UserControl x:Class="Melville.Pdf.WpfViewer.Home.HomeView"
             ...
             xmlns:controls="clr-namespace:Melville.Pdf.Wpf.Controls;assembly=Melville.Pdf.Wpf"
              >
    <controls:PdfViewer Source="C:\File.pdf"/>
</UserControl>

````
2. Using Skia Sharp -- save PDF page to a png file
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

For more examples and explanations please got to [QuickStart](Docs/QuickStart/Display.md)

# Who else contributed?
I made an effort to contain the dependencies as much as I can.  The goal is to be free, and that means I 
need to control the number of people who can make an ownership claim to the source code.  In the end I took
3 dependencies, as discussed below.
- [Freetype](http://freetype.org/license.html) via [SharpFont](https://github.com/Robmaister/SharpFont) is
used for font file parsing.  This is the only native code dependency in the project, and much as I know
it will make portability a headache, this was a necessity.  PDF allows a lot of different font formats and
writing parsers for them all would have added another year to the project.  This is a genuine NuGet 
dependency.
- [JpegLibrary](https://github.com/yigolden/JpegLibrary) is the source of my JPEG parsing code.  I "forked"
this library into my own codebase in Melville.JpegLibrary.  I have added some bugfixes and other features
to this code to make it parse some of the unusual JPEG images this project had introduced me to.
- My JPEG2000 parser is another "unofficial fork" of [csj2k](https://github.com/cureos/csj2k).  I made
some superficial modifications to make it library play nicer with the stream based filter paradigm which
is fairly fundamental to PDF,

I made significant efforts to keep Melville.Pdf agnostic with regard to rendering or display technologies.
To help me toward this end I developed the initial library with two initial targets, WPF and SkiaSharp.
Melville.Pdf.SkiaSharp is a nice and contained example of the limited interface needed to connect Melville.Pdf
to a display technology.  The WPF and SkiaSharp bindings clearly depend on Wpf and SkiaSharp, respectively. 

All of the dependences listed above have nonrestrictive open source style licenses which allow source and
binary royalty-free distribution, including usage in closed source and commercial projects.