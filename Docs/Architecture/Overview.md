﻿# Architcture Overview

## Assembly Summaries

Melville.Pdf is composed of a number of different assemblies.  A detailed description of each assembly
is [here.](AssemblySummaries.md)

In general if you want to use the library look at Melville.Pdf.Wpf or Melville.Pdf.Skia.  If you want
to manipulate the objects that make up the Pdf document, look at Melville.Pdf.LowLevel.  Parsers for the
formats included in PDF are located in Melville.ICC, Melville.JpegLibrary, Melville.Csj2k, Melville.CCITT,
and Melville.JBIG2.

## My Roslyn Tools
Roslyn, the C# compiler, supports 2 important user extensions.  Analyzers allow the compiler to enforce additional rules specific to a given project or dependency.  Code generators can examine source code and create additional code that sits along side the examined code.  I use both of these features in Melville.Pdf.
- [Code Generators](CodeGenerators.md)
- Async analyzer makes sure async methods are named appropriately.

## Patterns
Every codebase has its idiosyncrasies.  Because it has one principal author, I am sure that Melville.Pdf
has more than most.  In this section I will attempt to document some of the practices I use when I write
code, or techniques I developed for this project.

- [Why is everything async?](Async.md)
- [Costume Pattern](Costumes.md)
- [Names in Pdf](Names.md)