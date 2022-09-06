# Introducting Melville.PDF

The purpose of Melville.PDF is simple:  make PDF documents show up in .NET apps __for free__.
Free means free! Free as in speech?  Free as in beer? Who cares?  Free does not mean:
- There is an old version for free but the modern version is locked behind a paywall.
- The version is free if you agree to give your software away.
- It's free but it only works on Windows, or only on the desktop,
- The free version works for short files, or watermarks your files or anything else.
- The free lite version is so feature strapped that you will eventually have to buy the Pro version.

In short free means free.

I have recently become a fan of [Bob Martin](https://blog.cleancoder.com/) and his dogma surrounding
[Clean Code](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882).  You will
have to decide for yourself if my code is clean.  Jumping into any codebase is difficult.  I added some 
introductory material under [Architecture](Docs/Architecture/Overview.md) to  orient you to the codebase.

# Enough Gabbing -- Show Me Some Code!

1. The Wpf Way -- Show PDF in a Control
````xaml
<UserControl x:Class="Melville.Pdf.WpfViewer.Home.HomeView"
             ...
             xmlns:controls="clr-namespace:Melville.Pdf.Wpf.Controls;assembly=Melville.Pdf.Wpf"
              >
    <controls:PdfViewer Source="C:\File.pdf"/>
</UserControl>

````
2. Using Skia Sharp -- save PDF page to a png file
````c#

````