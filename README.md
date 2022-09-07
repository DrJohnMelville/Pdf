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

# What does Melville.Pdf not do well?
This first release of Melville.Pdf is really focused on one goal, to render
PDF documents in .NET applications.  Years ago, someone whom I cannot
remember told me that ["To Ship is to Choose."](https://devblogs.microsoft.com/powershell/diy-ternary-operator/)  Here are the things I chose
not to include so I could ship the first version.
- 256 bit encryption
- Public Key Encryption
- Types 4-7 Shader patterns
- Document navigation features like document outlines or page thumbnails.
- Pdf Generation.
    - Melville.Pdf actually has fairly robust PDF generation
      capabitites, but they are part of the test suite.  I generate lots of different
      PDF files in my integration testing.  The problem is I do not need to do
      much PDF Generation, and I do not a "customer" to drive a rational design.
      The result, I am sure, is a PDF generation framework much more focused on
      ability to produce unusual variants of PDF syntax over anything that might
      be useful in creating real documents.
- Forms of any kind.
- Text or Graphics extraction

# What's next on the roadmap?

This first version -- 0.1 is technically a beta.,  I have tried to keep the
code quality high, but you will have to judge that.  I hope to continue this
project.  Here are things I am likely to take this up next.

- Quality and Pdf 2.0 Compliance
    - A year and a half into this project I finally ponied up the $200 for
      the [ISO 32000:2](https://www.pdfa.org/resource/iso-32000-pdf/) specification
      docuument.  My next step is going to be a page by page trip through the relevant
      spec to include the 2.0 PDF enchancements, including 256 bit encryption support.  I also plan to
      use this as a quality milestone.  I want to include more test coverage with examples from the specification
      and so forth.
    - Right now just about everything in the library is public.  After re-reading Microsoft's
      [Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/) I
      decided that Melville.Pdf isn't a framework, it's a library.  You use it to render PDF's, and I doubt
      many of my users will care much about the internal details of PDF or the library.  Shifting most of my
      mechanisms to internal classes may prevent breaking changes later on in the life of the library.  I will
      allow my self until version 1 to make as much as makes sense into internal classes.
- Text and image extraction.  I need this for another project at work.
- Filling and extracting text from Adobe and XFA forms -- another feature I could use at work.
- Document outline and page thumbnails
- More work on the Wpf viewer control to support at least zoom and rotation.

I am extremely fortunate to have good employment as a physician at the
[Medical University of South Caroline](https://medicine.musc.edu/departments/pediatrics/divisions/child-abuse-pediatrics/faculty)
meaning that programming can be just a hobby for me.  I will probably work on the things above as time allows.
My commit history ought to convince you that I really like programming.  I take my amateur status
seriously, though.  It means I take the time to write the code correctly.  I am not committing to any specific
roadmap or schedule.

# How can I help?
The biggest thing I need right now is bug reports.  I am especially interested in examples of PDF files that
either crash the renderer or render incorrectly.  You can post such files as issues on git hub or email them
to me at JohnMelvile@gmail.com.

If one of the topics above interests you and you are a C# programmer, please drop me some email.  Right now
the codebase is largely the product of a single contributor and the code is pretty cohesive.  I would enjoy
collaborating with other developers who want to code in a similar style, and don't mind rather heavy edits
from an opinionated maintainer.

More than anything else what I crave right now is users.  Try out the library and tell me what works and
what doesn't.  Tell me your good ideas, I may like them more than my own.  If there is some small issue that
is blocking you let me know.  Right now I'm a amateur with no budget, no schedule, and no users.  If there
is somthing that would win me some real users -- you just might find yourself at the top of the backlog!