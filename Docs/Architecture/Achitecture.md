# The Architecture Analyzer

Bob Martin's book [Clean Architecture](https://www.amazon.com/Clean-Architecture-Craftsmans-Software-Structure/dp/0134494164) proposes, in addition to others, two principles for reusable software components.  The ayclic dependency principal specifies that modules in a
system should not depend on modules that transitively depend on them.  Furthermore, external dependencies should be carefully cabined with in the code and promptly wrapped with your own abstractions.  That way if your dependencies change, only a very small part of the code will have to change.

Normally the bounds of a component in c# is the assembly, which may not be the size of the logical component.  Creating assemblies has some cost in terms of discoverability and runtime performance.  I wrote the architecture analyzer to enforce architectural layering within an assembly.

## Defining dependency groups

The analyzer looks at the root of the project for a file named Architecture.adf.  Here is the Architecture.Adf file for Melville.Pdf.LowLevel

````
#Layering
Group Parser
    Melville.Pdf.LowLevel.Parsing.*
    Melville.Pdf.LowLevel.Encryption.*
Group Model
    Melville.Pdf.LowLevel.Model.*
    Melville.Pdf.LowLevel.Visitors.*
    Melville.Pdf.LowLevel.Filters.*
Group Writers
    Melville.Pdf.LowLevel.Writers.*
    Melville.Pdf.LowLevel.Encryption.*
Parser => Model
Writers => Model

#Everyone can use various external resources
* => System.*
* => Melville.Parsing.*

#JpegLibrary
Melville.Pdf.LowLevel.Filters.ExternalFilters.* ^=> Melville.JpegLibrary.*

#CSj2K
Melville.Pdf.LowLevel.Filters.JpxDecodeFilters.* ^=> Melville.CSJ2K.*

#CCITT
Melville.Pdf.LowLevel.Filters.CCITTFaxDecodeFilters.* ^=> Melville.CCITT.*

#JBIG2
Melville.Pdf.LowLevel.Filters.Jbig2Filter.* ^=> Melville.JBig2.*
````
The #Layering section describes architectural layers, in this case a parer, model, and writers layer.  Within each layer each line specifies (using globs where appropriate) the complete type names contained within each group.  Any lines that begin with whitespace are added to the preceding group.  Types within a group can reference one another.

In the next two lines the => operator indicates that the Parser depends on the model and Writers depends on the model.  Thus Parsers and Writers can use or refer to types in the Models group but Models cannot see the Parser or Writers.  Model classes may not name any types in the forbidden groups, call methods that require or return a forbidden type, or use a generic type specialized with a forbidden type.  The architecture analyzer enforces the layering constraints within an assembly.

## Cabin External Dependencies

The remaining lines are used to cabin the dependency on external libraries to the minimum usable scope.  The ^=> operator grants the left hand argument exclusive access to the resource on the right.

For example, Melville.Pdf.LowLevel depends on Melville.JpegLibrary.  Melville.JpegLibrary is the fourth library that has been used to read Jpegs.  Each time the change was trivial because the architecture analyzer ensures access to the jpeg library is exclusively limited to the namespace Melville.Pdf.LowLevel.ExternalFilters which contains a single class containing 19 lines of code.  Each time I switched libraries the changes were restricted to a single, trivial class.

For Reference the architecture analyzer recognizes the following operators.

| Operator     |Intepretation|
|--------------|---|
|  A.* => B.*  | A may reference B and B may not reference A |
 |  A.* !=> B.* | A may not reference B |
 |  !A.* => B.* | Types that do not match A may reference B |
 |  A.* ^=> B.* | Only types matching A may reference B |
 |  A.* <=> B.* | A may reference B and B may reference A |