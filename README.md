# Pdf

This may be a pipe dream, but it may turn into something I have wanted for a long time -- a liberally licensed, open source PDF reader and renderer for .NET.  I am sick of "almost free" or "tied to windows" or "you have to use AGPL if you want the modern version." PDF libraries in .NET.

The other significant value I espouse if clean code, as chambioned by Bob Martin.  PDF is an intricate and complicated format.  I hope to provide APIs at different levels with clean architectural boundaries between them.  I hope to isolate code into small reusible classes.  I hope to support the effort with an excellently engineered test suite.

A clean architecture means not being tied to any particular UI or any particular framework.  I love WPF, but an interested by .net MAUI.  I also write server side code, so you souldn't have to have a UI dependency at all.  That means a lot of small projects.  I hope you will be able to use the parts you want and ignore the rest.

So it might be a pipe dream: but here we go.
