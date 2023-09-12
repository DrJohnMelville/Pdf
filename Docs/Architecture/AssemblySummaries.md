# Assembly Summaries

## Renderers
Renderers are the assemblies that most consumers will want to depend upon directly.  Select the appropriate
renderer for your output technology and the remaining necessary assemblies will be included as transitive
dependencies.

**Melville.Pdf.FontLibrary** implements a single class, SelfContainedDefaultFonts, and contains multple font
resources to allow rendering of the 14 default PDF fonts without using the system font library.  Using this
library trades reliability for executable size, and so it is packaged as a separate assembly.

**Melville.Pdf.SkiaSharp** supports rendering PDF content to a [SkiaSharp](https://github.com/mono/SkiaSharp) SkSurface
object.  [Skia](https://skia.org/) itself
targets [multiple output formats](https://en.wikipedia.org/wiki/Skia_Graphics_Engine) including raster image
files, OpenGL, ANGLE, Vulan, Metal, SVG, XPS, and even PDF.  The SkiaSharp renderer is the simpler of the
two renderers originally built along with the generic rendering engine.

**Melville.Pdf.Wpf** is the more complicated of the two renderers.  WPFs retained-mode graphics system
required some intricate caching to get acceptable performance in text rendering.  This assembly also
provides WPF controls to display a PDF.  PageDisplay will dsplay a single pdf page with no chrome.  PdfViewer
provides minimal reader chrome to flip between pages and show optional layers in a pdf document.

**Melville.Pdf.ImageExtractor** is a special renderer that extracts the images from the pages as it renders.

**Melville.Pdf.TextExtractor** "renders" pdf pages to strings from the rendered page.

## The Pdf Libraries
These assemblies implement the core of the PDF standard, and are really the heart of Melville.PDF.

**Melville.Pdf.LowLevel** implements the lexcial parsing of PDF into its lexical structure as a graph of
PDF objects.  This roughly corresponds to an implementation of chapter 7 of the PDF specification with the
exception of section 7.7, which is implemented in Melville.Pdf.  The PdfLowLevelReader struct will parse
a variety of input formats into a PdfLowLevelDocument class.

**Melville.Pdf.Model** builds on top of the object graph defined in Melville.Pdf.LowLevel to present a PDF
document as a sequence of pages that can be rendered.  Most of the code in this assembly is dedicated to
rendering as specified in chapters 8, 9, and 10 of the PDF specification.  Melville.Pdf.Model defines
IDrawTarget, IRenderTarget, and IGraphicsState interfaces that are the target of PDF drawing operations.  The
renderer assemblies implement these interfaces using various output technologies.  The PdfReader class is
defined in Melville.Pdf.Model.  PdfReader parses a variety of input objects to a DocumentRenderer class.

**Pdf.KnownNameGenerator** is a compile time library that defines a number of source generators which
generate parts of Pdf.LowLevel.  This includes the KnownName constants, content stream operator constants,
postscript interpreter operator classes, and the standard character mappings.

## The Subordinate Parsers
The PDF standard relies on many file formats defined outside of the PDF specification.  Many of these formats
might be useful outside of the context of PDF parsing.  For convenience, these parsers are packaged as
independent assemblies.

**Melville.Parsing** is a utility library that contains code common to many of the parser implementations
throughout Melville.Pdf.

**Melville.ICC** parses International Color Consortium (ICC) profiles and performs color space conversions as
specified in ICC.1:2004-10.

**Melville.JpegLibrary** reads the JPEG stream format.  This assembly was copied from yigolden's
[JpegLibrary](https://github.com/yigolden/JpegLibrary).  I have fixed some bugs in monochrome JPEG decoding
and recognizing the App14 (0xFF0E) tag to define the Jpeg colorspace.

**Melville.CSJ2k** reads Jpeg2000 image files.  This assembly was copied from cureos'
[csj2k](https://github.com/cureos/csj2k).  I have made very light changes to this assembly in order to get
raw bitstreams without excessive copying or needless memory consumption.

Both Melville.Jpeg and Melville.CSJ2k have potential for better integration with the rest of the library in
the future.  CSJ2k has a limited ICC engine that could be replaced with Melville.ICC.  Both use arithmetic
decoders which duplicate the arithmetic decoder in the JPX parser.  Both could be better integrated with
the async pattern used throughout the remainder of the the parser.

**Melville.CCITT** parses CCITT Fax encoded streams according to ITU Recommendations T.4 and T.6.

**Melville.JBig2** parses JBIG2 encoded streams according to ITU Recommendation T.86.  Melville.JBig2 depends
on Melville.CCITT.

**Melville.Postscript.Interpreter** implements a postscript language interpreter.  Multiple parsers including parsers for CMAPs, Type 4 Functions, and content streams are directly
implemented as postscript interpreters.  The  main PdfObject parser borrows a lot of code from the postscript interpreter, but has its own codebase.  The Pdf object parser stores
less information per object than postscript, and handles indirect references.

**Welville.Postscript.OperationGenerator** is a code generator assembly that facilitates creating operator libraries
for the postscript parser.

## The Testing Libraries

**Performance.Playground** is a Benchmark.Net Project.  I use this project intermittently when I do 
performance optimization.  It has some historical interest and the naive version of some of the optimized
algorithms, but it is not incredibly useful.

**Melville.Pdf.FuzzTest** has not yet made it all the way to fuzz testing.  It will search an entire 
directory tree for PDF files and then try to render every page of the file.  I point this at the root
of my documents folder to try and parse every pdf document I have, which has exposed many bugs.

**Melville.Pdf.ReferenceDocuments** is a collection of classes that generate pdf documents, often documents
that explore different aspects of the PDF specification.  These are used in the comparing reader and in the
integration tests.

**Melville.Pdf.ReferenceDocumentGenerator** is a thin, command line UI to generate the PDF reference 
documents.  (This project predates the ComparingReader, and has been largely replaced by it.)

**Melville.Pdf.DataModelTests** is the main unit testing assembly.  The Standard folder contains subfolders
for many clauses of the PDF standard containing tests that verify compliance with those clauses.

**Melville.Pdf.WpfToolTest** is a small assembly with unit tests for code in the LowLevelReader and
Comparing Reader.

**Melville.Pdf.IntegrationTesting** contains unit tests that render each of the documents in the 
Melville.Pdf.ReferenceDocuments using both WPF and Skia renderers.  The resulting png files are hashed
compared to stored hashes.  This helps detect rendering regressions.  If you uncomment the 
````c#
    //hashes.Recording = true
````
line in the RenderingTest, the tests will record, rather than compare the hashes.  This makes it easy to
verify that a test works in the ComparingReader and then record hashes to ensure that the rendering does 
not change.

## The Tools
**Melville.Pdf.ComparingReader** is used to debug rendering.  The tree at the left contains all of the
Reference Documents, or you can load documents from the file system.  

To the right a number of renderers render the selected page of the document.  Current
renders include the windows SDK pdf renderer, the Melville.PDF.WPF renderer using the default windows fonts,
the Melville.Pdf.SkiaSharp renderer using the Melville.Pdf.Fontlibrary fonts, a low level view of the page,
all the images from the page, all the text from the page,
and a page that will invoke the system registered PDF viewer.  

A PDF "REPL" feature allows you to edit content streams and immediately see the result in multiple renderers.
The low level view can save streams (compressed or uncompressed) from the PDF file to the file system.

This is the primary tool I use to hunt for bugs in the renderers.  This app is quite slow as every page 
change invokes multiple renderers over and over again.

**Melville.Pdf.LowLevelReader** views PDFs as a a set of objects.  Contrary to what its name would imply, the
lowlevel reader actually depends on complete WPF renderer to provide previews of pages and other high level
Pdf object.  The low level reader is a tab in the comparing reader, and a rarely use it on its own.

**Melville.Pdf.LowLevelViewerParts** is a library class for the shared code between
Melville.Pdf.ComparingReader and Melville.Pdf.LowLevelReader.  

**Melville.Pdf.WpfViewer** is a minimal shell around the WPF PdfViewer control.  It is minimally functional
as a PDF reader.  This app also gives a pretty good idea of what the performance of the renderer will be, so
I use it for informal "good enough" performance testing. 