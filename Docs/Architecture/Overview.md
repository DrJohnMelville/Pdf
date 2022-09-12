# Architcture Overview

## High Level Overview

Melville.Pdf is composed of a number of different assemblies.  A detailed description of each assembly
is [here.](AssemblySummaries.md)

In general if you want to use the library look at Melville.Pdf.Wpf or Melville.Pdf.Skia.  If you want
to manipulate the objects that make up the Pdf document, look at Melville.Pdf.LowLevel.  Parsers for the
formats included in PDF are located in Melville.ICC, Melville.JpegLibrary, Melville.Csj2k, Melville.CCITT,
and Melville.JBIG2.

## Patterns
- [Costume Pattern](Costumes.md)