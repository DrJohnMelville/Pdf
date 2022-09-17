# Why is there so much async code?

Internally a PDF document is a collection of PDF objects.  The, simplified, top level organization of a PDF 
file looks something like this
````
%PDF-1.6
1 0 obj [1 true 5.6 null]
2 0 obj << /Type /Font /SubType 1 0 R >>
... %other objects
xref
1 2
0000000009 00000 n
0000000038 00000 n
... %other object locations
startxref
110
%%EOF
````

The file begins with a header line followed by a list of numbered object.  The object numbered (1,0) is an
array with elements of 1, true, 5.6, and null.  Object (2,0) is a dictionary of keys and their associated
values.  Following the list of objects is an xref table that tells us that object (1,0) starts at position
9 and object (2,0) starts at position 38.

A Pdf file can easily run thousands of pages and multiple gigabytes.  Clearly we cannot parse all the 
objects before displaying pages.  Upon opening a file, Melville.PDF parses the XRef table and uses it
to locate objects lazily as needed.

Take another look at object number 2.  The first element of the dictionary has a key of /Type and a value of 
/Font.  (By convention names in PDF are written with a preceeding forward slash.)  The second element of 
the dictionary has a key of /SubType but the value is the array stored in the object numbered (1,0).
  The 1 0 R syntax is a pdf reference object.

In PDF object references can be used almost anywhere an object can be used.  I have seen codebases where this
fact contaminates the entire codebase -- everywhere you use a value you have to check if it is a 
reference and look up the target if it is.  This seems to me like a farm of bugs waiting to happen.  There are
complicated rules about references being required or disallowed in various contexts -- but depending on them
just feels brittle to me.  (There is one restriction I do utilize.  The keys of a dictionary must be
PDF name objects and cannot be references.)

I chose a different architecture.  PdfArray and PdfDictionary transparently resolve any references in the
element access operations.  Since every object (other than the document root) is accessed through either
a dictionary or an array, 99% of the codebase is completely ignorant that references exist at all.  Pdf 
reference objects also cache their values so each PDF object will be read from the disk only once.

The downside of this design is that any array dereference or dictionary lookup could potentially touch the
source data if it is a reference to an object that has not been read yet.  Because any array or dictionary
access could touch the disk, virtually every method that interacts with the PDF object model would have to
be async because literally any member access could touch the disk.

The reason the above paragraph does not turn into a performance nightmare is because the overwhelming
majority of member accesses do not touch the disk -- either because the object was declared inline, or it 
is an object reference that has been cached.  The pervasive use of ValueTask makes these reads relatively
inexpensive compared to using Tasks.