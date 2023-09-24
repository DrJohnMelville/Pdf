using System.Threading.Tasks;
using Melville.Pdf.LowLevel.Model.CharacterEncoding;
using Melville.Pdf.LowLevel.Model.Conventions;
using Melville.Pdf.LowLevel.Model.Objects;
using Melville.Pdf.Model.Documents;
using Melville.Pdf.Model.Renderers.FontRenderings.CharacterReaders;
using Melville.Pdf.Model.Renderers.FontRenderings.CMaps;
using Moq;
using Xunit;

namespace Melville.Pdf.DataModelTests.CmapParsers;

public class ParsedCMaps: IAsyncLifetime
{
    public IReadCharacter[] Maps { get; } = new IReadCharacter[3];

    public async Task InitializeAsync()
    {
        Maps[0] = await ParseMapAsync(CMapFromCmapSpec);
        Maps[1] = await ParseMapAsync(UnicodeCMapFromSpec);
        Maps[2] = await ParseMapAsync(BrokenFromPdfFile);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private async ValueTask<IReadCharacter> ParseMapAsync(string text)
    {
        PdfEncoding encoding = new PdfEncoding(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.CMap)
            .AsStream(text));
        IRetrieveCmapStream library = Mock.Of<IRetrieveCmapStream>();
        return await new CMapFactory(
                GlyphNameToUnicodeMap.AdobeGlyphList, TwoByteCharacters.Instance, library)
            .ParseCMapAsync(encoding.LowLevel);
    }

    private const string BrokenFromPdfFile = """
         /CIDInit /ProcSet findresource begin
         12 dict begin
         begincmap
         /CMapType 2 def
         /CMapName/R100 def
         1 begincodespacerange
         <0000><ffff>
         endcodespacerange
         29 beginbfrange
         <01><01><0020>
         <02><02><0041>
         <03><03><0062>
         <04><05><0073>
         <06><06><0072>
         <07><07><0061>
         <08><08><0063>
         <09><09><003a>
         <0a><0a><0049>
         <0b><0c><006e>
         <0d><0d><0064>
         <0e><0e><0075>
         <0f><0f><0069>
         <10><10><0044>
         <11><11><006d>
         <12><12><0065>
         <13><13><0054>
         <14><14><0067>
         <15><15><0066>
         <16><16><0053>
         <17><17><0078>
         <18><18><006c>
         <19><19><0079>
         <1a><1a><0070>
         <1b><1b><0050>
         <1c><1c><0068>
         <1d><1d><004c>
         <1e><1e><0046>
         <1f><1f><0043>
         endbfrange
         endcmap
         CMapName currentdict /CMap defineresource pop
         end end

         """;

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

        2 beginnotdefrange
        <00> <1f> 0
        <8148> <9FFC> 16#8149
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

    //This comes from PDF 2.0 Spec section 9.10.2
    private const string UnicodeCMapFromSpec = """
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
                                               <0000> <005E> <12340020>
                                               <005F> <0061> [<00660066> <00660069> <00660066006C>]
                                               endbfrange
                                               1 beginbfchar
                                               <3A51> <D840DC3E>
                                               %0397;Eta;GREEK CAPITAL LETTER ETA
                                               <3A52> /Eta
                                               endbfchar
                                               endcmap
                                               CMapName currentdict /CMap defineresource pop
                                               end
                                               end
                                               """;
}