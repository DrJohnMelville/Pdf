# Releases

## 1/16/2025 0.6.0
This fixes some bugs
- Fixed an end condition in a linked list class deep in the font parser.
- Parses true type font files that (contrary to spec) do not sort their tables properly.
- Parses true type font files that try (contrary to spec) to declare more horizontal metrics than there are glyphs.
- Fixed a concurrany bug in the WPF glyph caching mechanism.
- Upgrade to .NET 9.0 and fix all the warnings in the new version.
- Cap fluentassertions at version 7.0.0 (the last free version)
- Update dependencies, including some dependencies with vulnerabilities.


## 9/13/2024 0.5.1
This is a minor bugfix release.
- Fixed a bug in the JBIG2 decoder that caused a crash when the JBIG2 segment length was unknown.
- Fixed a bug with RentedStream not honoring the declared end of stream.
- Fixed a bug when /Pattern is used as a colorspace.
- Fixed a bug where rendering a documentrenderer on different renderers could cause cached fonts to be of a type
the second renderer was not expecting.

## 9/5/2024 0.5.0
This is a major feature release.
- The native SharpFont dependency is replaced with the new Melville.Fonts font parser.
- Rendering fonts fast enough drove a lot of performance work in the low-level file abstractions.
- IMultiplexSource has different specializations for files, static arrays (which happens a lot in test)
or expandable arrays.  It cann also serialize access to a stream.  Specifically, all these byte sources
are now reliably disposed, which lets them return their buffers the to array pool correctly.

## 5/15/2024 0.4.8
This is still a service release working toward the WinId for the Web integration.
- Fixed a bug parsing a JBIG file that has no global segment.
- Fixed a hang bug in the jpeg2000 reader
- fixed a parsing bug when a page has multiple content streams and there is no witespace between them.
- Fixed parsing of pdf files that have an invalud CMAP
- There are now special cases for parsing file streams, memory streams, and multibuffer streams that do 
not use locks o acheive thread safety.  All three of these structures can be read from multiple locations
simultaneously on different threads.  There is no API change, the parser just notices these special streams and
reads them in a way that it does not need to lock other readers to acheive thread safety.

## 4/20/2024 0.4.7
Not a lot done here recently.  I have been working over the past 6 months to get a release of WinId for the Web 
ready for the for the forensic odontologists.  I have also been using the pdf renderer every day at work.  This
release is just the bugs I ran into and fixed in 6 months of daily use at work.
- Fixed XFA form reader consistently uses utf-8 encoding within form fields.
- JPEGs that have no undersampling render with the correct YUV->RGB transform.
- Character maps with empty ranges (that are forbidden by the spec) do not crash the reader.

## 11/22/2023 0.4.6
- Add the ability to read and write AcroForms and XfaForms.
- Cannot display any kind of forms, can just manipulate the form data using the Melville.Pdf.FormReader assembly.
- Adopt .Net 8.0

## 10/10/2023 0.4.5
- Fixed a bug when a JPEG stream was subsequently encoded in a flate stream.
- Performance improvement in bitmaps that have a custom color mapping. 
- For those who have noticed the increase in release cadence.  I am using Melville.PDF in my main project
at work, so the releases are a little closer together as I need to have functioning software at work.

## 9/22/2023 0.4.4
- Melville.SharpFont can find its native binaries when compressed into an architecture-specific project.

## 9/22/2023 0.4.3
- Another attempt to fix the bug that version 0.4.2 did not actually fix.

## 9/19/2023 0.4.2
- Fix a bug that Melville.SharpFont's nuget package does not contain the needed unmanageqd binaries.

## 9/18/2023 0.4.1
- Fix a bug where trying to write a string containing a single { parses incorrectly in content streams.

## 9/12/2023 0.4
- ImageExtractor and TextExtractor get the images and text, respectively from PDF files.
- A new implementation of the fundamental pdf objects significantly reduces the number of
GC objects used in rendering PDF.
- A dedicated postscript interpreter exists in a separate assembly.  A number of parsers throughout
the library have been re-written as postscript variants.  This means that some illegal PDF will 
parse.  (For example explicit-radix numbers from postscript work in the PDF parser even though the PDF
standard does not allow them.  Since this syntax is illegal in PDF, the fact that it happens to work is 
harmless because every legal PDF file parses correctly.  Detecting invalid PDF syntax is not a goal of this
project.)

## 5/9/2023 0.3
- I made a whole bunch of stuff internal.  It is hoped that this will prevent breaking changes later on because 
no one will depend on the internal bits.  There may be some breaking changes, but I am taking them now when I hope
they will not hurt too much.  If I broke your use case, let me know.
- Every externally visible class and member has an xml doc comment.

## 12/17/2022 0.2
- Pdf 2.0 Compliance improvements throughout the parser.
- 256 bit AES encryption and decryption.
- Honor ColorTransform on a Dct encoded images.
- Update text rendering to use algorithm from 2.0 spec which covers a number of corner cases, including negative font sizes.
- Caching some intermediate values in shaders results in signifcant performance improvement.
- Detect some cases where invalid PDFs result in stack overflows.
- General improvements in code quality.

## 9/24/2022 0.1
- Fix a Wpf Rendering deadlock bug.
- Fix Version numbers to reflect semantic versioning.

## 9/17/2022 0.0.1
Initial Release