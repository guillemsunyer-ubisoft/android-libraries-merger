# Android libraries merger
This is a small program for manually resolving Android libraries with Unity. This is desired when the automatic Android Resolver fails.

To use, download the release exe, and execute it from the command line with two parameters. 
- The first one is the path to the folder that contains the libraries that need to be added.
- The second one is the path to the folder that contains the libraries where the libraries from the first path will be merged.

For example:
```
PathToTheExe/LibsMerger.exe D:/User/SDKsUpdate/LibrariesToAdd/ D:/User/SDKsUpdate/LibrariesWhereTheAddedLibrariesWillBeMerged/
```
