namespace Melville.Pdf.DataModelTests.CmapParsers;

public class CmapToCodeTest
{
    private const string CMapFromCmapSpec =
        """
        %!PS-Adobe-3.0 Resource-CMap
        %%DocumentNeededResources: ProcSet CIDInit
        %%IncludeResource: ProcSet CIDInit
        %%BeginResource: CMap (CJKTypeBlogTest-UTF32-H)
        %%Title: (CJKTypeBlogTest-UTF32-H Adobe Identity 0)
        %%Version: 1.000
        %%EndComments

        /CIDInit /ProcSet findresource begin

        12 dict begin

        begincmap

        /CIDSystemInfo 3 dict dup begin
          /Registry (Adobe) def
          /Ordering (Identity) def
          /Supplement 0 def
        end def

        /CMapName /CJKTypeBlogTest-UTF32-H def
        /CMapVersion 1.000 def
        /CMapType 1 def

        /WMode 0 def

        /UUIDOffset 0 def
        /XUID [1 10 25324] def


        1 begincodespacerange
          <00> <80>
          <8140> <9ffc>
          <a0> <df>
          <a040> <fbfc>
        endcodespacerange

        1 beginnotdefrange
        <00> <1f> 0
        endnotdefrange

        3 begincidrange
        <20> <7E> 1
        
        endcidrange

        1 begincidchar
        <7F> 15
        endcidchar

        endcmap
        CMapName currentdict /CMap defineresource pop
        end
        end

        %%EndResource
        %%EOF
        """;
}
public class CmapToUnicodeTest
{
    //This comes from PDF 2.0 Spec section 9.10.2
    private const string CMapFromSpec = """
        /CIDInit /ProcSet findresource begin
        12 dict begin
        begincmap
        /CIDSystemInfo
        <</Registry (Adobe)
        /Ordering (UCS2)
        /Supplement 0
        >> def
        /CMapName /Adobe-Identity-UCS2 def
        /CMapType 2 def
        1 begincodespacerange
        <0000> <FFFF>
        endcodespacerange
        2 beginbfrange
        <0000> <005E> <0020>
        <005F> <0061> [<00660066> <00660069> <00660066006C>]
        endbfrange
        1 beginbfchar
        <3A51> <D840DC3E>
        endbfchar
        endcmap
        CMapName currentdict /CMap defineresource pop
        end
        end
        """;
}