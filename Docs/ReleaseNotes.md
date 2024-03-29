﻿# Releases

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