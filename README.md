# Android libraries merger
## What is it
This is a small program for manually resolving Android libraries with Unity. This is desired when the automatic Android Resolver fails.

## How to use
To use, download the release exe, and execute it from the command line with two parameters. 
- The first one is the path to the folder that contains the libraries that need to be added.
- The second one is the path to the folder that contains the libraries where the libraries from the first path will be merged.

Libraries will be searched in a recursive manner inside the provided paths.

For example:
```
PathToTheExe/LibsMerger.exe D:/User/SDKsUpdate/LibrariesToAdd/ D:/User/SDKsUpdate/LibrariesWhereTheAddedLibrariesWillBeMerged/
```

## Configuration
You can also add a configuration file with this format:
- The file needs to be named `libraries-merger.config`
- Needs to be on the same directory where the .exe is running.
  
```xml
<?xml version="1.0"?>
<ignore>
   <entry name="com.google.guava.guava"/>
   <entry name="org.jetbrains.kotlinx.kotlinx-coroutines-core"/>
</ignore>
```


![image](https://github.com/guillemsunyer-ubisoft/android-libraries-merger/assets/166832723/33bce9cd-863f-4f6f-920e-0d8de8501c18)


