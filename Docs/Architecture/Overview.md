# Architcture Overview

## High Level Overview

Melville.Pdf is composed of a number of different assemblies.  This section will explain the
various assemblies from high level to low level.

### Renderers
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

### The Pdf Libraries
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

### The Subordinate Parsers
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

## Patterns
- [Costume Pattern](Costumes.md)