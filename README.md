# csgen
A helper for building C# projects and generating clean code

# history
0.01 Hello World uploaded to github

0.09 Functionally have search-replace and nth-item replace

0.10 Added an insert text to a particular line function

0.11 Fixed error with replacenth where index out-of-range

0.14 generic parameters for model/vm creation

0.15 TBA



# Wishlist
I want this to be able to:
1 - do simple search-replace tasks in local files so that I can ditch replace.vbs (COMPLETE)

2 - do code generation for controllers/models so that I can use MVVM-style coding with a facade (which I like for personal preference) (WIP)

3 - do code generation with a consistent CapiTaliSation system (lowercase for viewmodels and controller actions, Titlecase for data objects, UPPERCASE for SQL, hungarianCase for models) (COMPLETE)

4 - generate views using Bootstrap 5 (ideally without jQuery) (WIP)

5 - script this to automate the creation of MVC websites (WIP)

6 - Help integrate Identity so that a functional, data-driven website with pretty Bootstrap can be built and functionally work locally in a single script (WIP)

7 - Run this on the Windows CLI (COMPLETE)


# Some caveats

This is not intended to replace MS commands like: dotnet aspnet-codegenerator identity -dc yq.Data.ApplicationDbContext -sqlite

The idea is that it works WITH the MS scaffolding tools and can be used in .BAT files 



