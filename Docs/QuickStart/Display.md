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