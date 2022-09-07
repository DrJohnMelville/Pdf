# How to Render Pdf

Melville.Pdf is designed to be a flexible PDF engine applicable in many 
contexts, both on the desktop and on the server.  Often you will interact
with various "bindings" to render PDF content in various formats.  At the
current time I have implemented 2 bindings: WPF and SkiaSharp.

1. The Wpf Way -- Show PDF in a Control
````xaml
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
- A Stream
- A PdfLowLevelDocument which presents the PDF file as a graph of objects.
- A PdfDocument, which is a thin ["Costume Type"](../Architecture/Costumes.md) around PdfLowLevelDocument
  that presents the PDF file as a sequence of pages.
- A DocumentRenderer which represents a PdfDocument of which some pages may have been previously rendered.
  The DocumentRenderer manages caching and the visibility of optional content.

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