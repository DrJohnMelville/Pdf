namespace Melville.Pdf.ReferenceDocuments.Text.CharSet.CMaps;

public class ExplicitCmap : BuiltinCMaps
{
    protected override PdfIndirectObject CreateEncodingFromClass(IPdfObjectCreatorRegistry registry)
    {
        return registry.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.CMap)
            .WithItem(KnownNames.CMapName, "/JDM-H")
            .WithItem(KnownNames.CIDSystemInfo, new DictionaryBuilder()
                .WithItem(KnownNames.Registry, "Melville")
                .WithItem(KnownNames.Ordering, "John")
                .WithItem(KnownNames.Supplement, 1)
                .AsDictionary())
            .AsStream("""
                      %!PS-Adobe-3.0 Resource-CMap
                      %%DocumentNeededResources: ProcSet (CIDInit)
                      %%IncludeResource: ProcSet (CIDInit)
                      %%BeginResource: CMap (JDM-H)
                      %%Title: (JDM-H Invert upper and lower case)

                      /CIDInit /ProcSet findresource begin

                      12 dict begin

                      begincmap

                      /CIDSystemInfo 3 dict dup begin
                        /Registry (Melville) def
                        /Ordering (John) def
                        /Supplement 1 def
                      end def

                      /CMapName /JDM-H def
                      /CMapVersion 10.006 def
                      /CMapType 1 def

                      /XUID [07 28 1975] def

                      /WMode 0 def

                      1 begincodespacerange
                        <0000>       <80FF>
                      endcodespacerange

                      1 beginnotdefrange
                      <0000> <001f> 0
                      endnotdefrange

                      3 begincidrange
                      <0020> <00FF> 1
                      <0041> <005A> 66
                      <0061> <007A> 34
                      endcidrange
                      endcmap
                      CMapName currentdict /CMap defineresource pop
                      end
                      end

                      %%EndResource
                      %%EOF

                      """));
    }
}

public class ExplicitCmapNamedDependency : BuiltinCMaps
{
    protected override PdfIndirectObject CreateEncodingFromClass(IPdfObjectCreatorRegistry registry)
    {
        return registry.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.CMap)
            .WithItem(KnownNames.CMapName, "/JDM-H")
            .WithItem(KnownNames.CIDSystemInfo, new DictionaryBuilder()
                .WithItem(KnownNames.Registry, "Melville")
                .WithItem(KnownNames.Ordering, "John")
                .WithItem(KnownNames.Supplement, 1)
                .AsDictionary())
            .AsStream("""
                      %!PS-Adobe-3.0 Resource-CMap
                      %%DocumentNeededResources: ProcSet (CIDInit)
                      %%IncludeResource: ProcSet (CIDInit)
                      %%BeginResource: CMap (JDM-H)
                      %%Title: (JDM-H Invert upper and lower case)

                      /CIDInit /ProcSet findresource begin

                      12 dict begin

                      begincmap

                      /CIDSystemInfo 3 dict dup begin
                        /Registry (Melville) def
                        /Ordering (John) def
                        /Supplement 1 def
                      end def

                      /CMapName /JDM-H def
                      /CMapVersion 10.006 def
                      /CMapType 1 def

                      /XUID [07 28 1975] def

                      /WMode 1 def

                      /Identity-H usecmap

                      2 begincidrange
                      <0000> <0075> 128
                      <0080> <00ff> 0
                      endcidrange
                      endcmap
                      CMapName currentdict /CMap defineresource pop
                      end
                      end
                      
                      %%EndResource
                      %%EOF
                      
                      """));
    }
}

public class ExplicitCmapDictionaryNamedDependency : BuiltinCMaps
{
    protected override PdfIndirectObject CreateEncodingFromClass(IPdfObjectCreatorRegistry registry)
    {
        return registry.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.CMap)
            .WithItem(KnownNames.CMapName, "/JDM-H")
            .WithItem(KnownNames.CIDSystemInfo, new DictionaryBuilder()
                .WithItem(KnownNames.Registry, "Melville")
                .WithItem(KnownNames.Ordering, "John")
                .WithItem(KnownNames.Supplement, 1)
                .AsDictionary())
            .WithItem(KnownNames.UseCMap, KnownNames.IdentityH)
            .AsStream("""
                      %!PS-Adobe-3.0 Resource-CMap
                      %%DocumentNeededResources: ProcSet (CIDInit)
                      %%IncludeResource: ProcSet (CIDInit)
                      %%BeginResource: CMap (JDM-H)
                      %%Title: (JDM-H Invert upper and lower case)

                      /CIDInit /ProcSet findresource begin

                      12 dict begin

                      begincmap

                      /CIDSystemInfo 3 dict dup begin
                        /Registry (Melville) def
                        /Ordering (John) def
                        /Supplement 1 def
                      end def

                      /CMapName /JDM-H def
                      /CMapVersion 10.006 def
                      /CMapType 1 def

                      /XUID [07 28 1975] def

                      /WMode 1 def

                      2 begincidrange
                      <0000> <0075> 128
                      <0080> <00ff> 0
                      endcidrange
                      endcmap
                      CMapName currentdict /CMap defineresource pop
                      end
                      end
                      
                      %%EndResource
                      %%EOF
                      
                      """));
    }
}

public class ExplicitCmapDictionaryExplicitDependency : BuiltinCMaps
{
    protected override PdfIndirectObject CreateEncodingFromClass(IPdfObjectCreatorRegistry registry)
    {
        var dependency = registry.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.CMap)
            .WithItem(KnownNames.CMapName, "/Identity-H")
            .WithItem(KnownNames.CIDSystemInfo, new DictionaryBuilder()
                .WithItem(KnownNames.Registry, "Adobe")
                .WithItem(KnownNames.Ordering, "Identity")
                .WithItem(KnownNames.Supplement, 0)
                .AsDictionary())
            .WithItem(KnownNames.UseCMap, KnownNames.IdentityH)
            .AsStream("""
                      %!PS-Adobe-3.0 Resource-CMap
                      %%DocumentNeededResources: ProcSet (CIDInit)
                      %%IncludeResource: ProcSet (CIDInit)
                      %%BeginResource: CMap (Identity-H)
                      %%Title: (Identity-H Adobe Identity 0)
                      %%Version: 10.006
                      %% This is not the official CMAP.  Melville.PDF implements an extension to CID Ranges in that it allows
                      %% CidRangers to cross the last byte boundary.  Because of this, a much more compact representnation is 
                      %% possible.
                      %%EndComments
                      
                      /CIDInit /ProcSet findresource begin
                      
                      12 dict begin
                      
                      begincmap
                      
                      /CIDSystemInfo 3 dict dup begin
                        /Registry (Adobe) def
                        /Ordering (Identity) def
                        /Supplement 0 def
                      end def
                      
                      /CMapName /Identity-H def
                      /CMapVersion 10.006 def
                      /CMapType 1 def
                      
                      /XUID [1 10 25404 9999] def
                      
                      /WMode 0 def
                      
                      1 begincodespacerange
                        <0000> <FFFF>
                      endcodespacerange
                      
                      1 begincidrange
                      <0000> <00ff> 0
                      endcidrange
                      endcmap
                      CMapName currentdict /CMap defineresource pop
                      end
                      end
                      
                      %%EndResource
                      %%EOF
                      
                      """));
        
        return registry.Add(new DictionaryBuilder()
            .WithItem(KnownNames.Type, KnownNames.CMap)
            .WithItem(KnownNames.CMapName, "/JDM-H")
            .WithItem(KnownNames.CIDSystemInfo, new DictionaryBuilder()
                .WithItem(KnownNames.Registry, "Melville")
                .WithItem(KnownNames.Ordering, "John")
                .WithItem(KnownNames.Supplement, 1)
                .AsDictionary())
            .WithItem(KnownNames.UseCMap, dependency)
            .AsStream("""
                      %!PS-Adobe-3.0 Resource-CMap
                      %%DocumentNeededResources: ProcSet (CIDInit)
                      %%IncludeResource: ProcSet (CIDInit)
                      %%BeginResource: CMap (JDM-H)
                      %%Title: (JDM-H Invert upper and lower case)

                      /CIDInit /ProcSet findresource begin

                      12 dict begin

                      begincmap

                      /CIDSystemInfo 3 dict dup begin
                        /Registry (Melville) def
                        /Ordering (John) def
                        /Supplement 1 def
                      end def

                      /CMapName /JDM-H def
                      /CMapVersion 10.006 def
                      /CMapType 1 def

                      /XUID [07 28 1975] def

                      /WMode 1 def

                      2 begincidrange
                      <0000> <0075> 128
                      <0080> <00ff> 0
                      endcidrange
                      endcmap
                      CMapName currentdict /CMap defineresource pop
                      end
                      end
                      
                      %%EndResource
                      %%EOF
                      
                      """));
    }
}