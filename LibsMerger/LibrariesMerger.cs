using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace LibsMerger;

public class LibrariesMerger(string directory1, string directory2)
{
    private static readonly string[] ValidExtensions = [".aar", ".jar"];

    public void Run()
    {
        LogLineJump();
        LogInfo($"WELCOME TO THE LIB MERGER");
        LogLineJump();
        
        DirectoryInfo directoryInfo1 = new DirectoryInfo(directory1);
            
        if (!directoryInfo1.Exists)
        {
            LogError($"Directory {directory1} does not exist");
            return;
        }
            
        DirectoryInfo directoryInfo2 = new DirectoryInfo(directory2);
            
        if (!directoryInfo2.Exists)
        {
            LogError($"Directory {directory2} does not exist");
            return;
        }

        LibrariesMergerConfiguration configuration = new();
        configuration.Load();

        if (!configuration.Loaded)
        {
            LogInfo("Configuration not found or with invalid format");
        }
        else
        {
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("Configuration found:");

            foreach (string toIgnore in configuration.ToIgnore)
            {
                stringBuilder.AppendLine($"- Ignoring {toIgnore}");
            }
            
            LogInfo(stringBuilder.ToString());
        }
        LogLineJump();

        
        LogInfo($"Resolving from directory {directory1} to {directory2}");
        LogLineJump();

        List<string> allFiles1 = GetAllFilesRecursive(directory1);
        List<string> allFiles2 = GetAllFilesRecursive(directory2);
            
        LogInfo("FILES TO MERGE: ==========================");

        foreach (string file in allFiles1)
        {
            LogInfo($"-{file}");
        }
            
        LogInfo("===================================");
        LogLineJump();
        
        LogInfo("FILES AT DESTINATION: ==========================");

        foreach (string file in allFiles2)
        {
            LogInfo($"-{file}");
        }
        
        LogInfo("===================================");
        LogLineJump();
        
        LogInfo("RESULT ===================================");

        List<Library> libraries1 = ProcessLibraries(allFiles1);
        List<Library> libraries2 = ProcessLibraries(allFiles2);

        bool foundDuplicatedLibraries1 = CheckForDuplicatedLibraryNames(libraries1, out DuplicatedLibrariesResult? duplicatedLibrariesResult1);

        if (foundDuplicatedLibraries1)
        {
            LogError($"Duplicated library name {duplicatedLibrariesResult1!.Value.Library1.Name} found at directory 1 {directory1}.\n" +
                     $"{duplicatedLibrariesResult1!.Value.Library1.Filepath} and {duplicatedLibrariesResult1!.Value.Library2.Filepath}");
            return;
        }
        
        bool foundDuplicatedLibraries2 = CheckForDuplicatedLibraryNames(libraries2, out DuplicatedLibrariesResult? duplicatedLibrariesResult2);

        if (foundDuplicatedLibraries2)
        {
            LogError($"Duplicated library name {duplicatedLibrariesResult2!.Value.Library1.Name} found at directory 2 {directory2}.\n" +
                     $"{duplicatedLibrariesResult2!.Value.Library1.Filepath} and {duplicatedLibrariesResult2!.Value.Library2.Filepath}");
            return;
        }

        List<MergeResult> mergeResults = MergeLibraries(configuration, libraries1, libraries2);

        IOrderedEnumerable<MergeResult> orderedMergeResults = mergeResults.OrderBy(o => o.ResultType);

        foreach (MergeResult mergeResult in orderedMergeResults)
        {
            string mergingVersion = string.IsNullOrEmpty(mergeResult.Merging.Version) ? "?" : mergeResult.Merging.Version;
            string targetVersion = string.IsNullOrEmpty(mergeResult.Target.Version) ? "?" : mergeResult.Target.Version;
            
            switch (mergeResult.ResultType)
            {
                case MergeResultType.Ignoring:
                {
                    LogInfo($"[{mergeResult.ResultType}] {mergeResult.Merging.Name}");
                    break;
                }
                case MergeResultType.Increased:
                {
                    LogInfo($"[{mergeResult.ResultType}] {mergeResult.Merging.Name}: {mergingVersion} > {targetVersion}");
                    break;
                }
                case MergeResultType.Same:
                {
                    LogInfo($"[{mergeResult.ResultType}] {mergeResult.Merging.Name} {mergingVersion} = {targetVersion}");
                    break;
                }
                case MergeResultType.Minor:
                {
                    LogInfo($"[{mergeResult.ResultType}] {mergeResult.Merging.Name} {mergingVersion} < {targetVersion}");
                    break;
                }
                case MergeResultType.New:
                {
                    LogInfo($"[{mergeResult.ResultType}] {mergeResult.Merging.Name} {mergingVersion} NEW");
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        LogInfo("===================================");
        
        LogLineJump();
        
        Console.WriteLine("Do you want to apply the merge to the files?\nPlease enter Y/y for Yes or N/n for No:");
        string userInput = Console.ReadLine();

        if (string.IsNullOrEmpty(userInput))
        {
            LogError("Empty input");
            return;
        }

        userInput = userInput.ToUpper();
        
        if (userInput.Equals("N"))
        {
            LogInfo("Bye :)");
            return;
        }
        
        if (!userInput.Equals("Y"))
        {
            LogError("Invalid input");
            return;
        }
        
        ApplyMerge(mergeResults);
        
        LogLineJump();
        LogInfo("All good, bye :)");
    }

    private static bool CheckForDuplicatedLibraryNames(List<Library> libraries, out DuplicatedLibrariesResult? duplicatedLibrariesResult)
    {
        for (int i = 0; i < libraries.Count; i++)
        {
            Library library = libraries[i];
            
            for (int y = 0; y < libraries.Count; y++)
            {
                bool isSameLibrary = i == y;
                
                if (isSameLibrary)
                {
                    continue;
                }
                
                Library checking = libraries[y];
                
                bool isDuplicated = library.Name.Equals(checking.Name);

                if (isDuplicated)
                {
                    duplicatedLibrariesResult = new DuplicatedLibrariesResult(library, checking);
                    return true;
                }
            }
        }

        duplicatedLibrariesResult = default;
        return false;
    }

    private void ApplyMerge(List<MergeResult> mergeResults)
    {
        string newDirectory = Path.Combine(directory2, "NewLibraries/");
        Directory.CreateDirectory(newDirectory);

        foreach (MergeResult mergeResult in mergeResults)
        {
            if (mergeResult.ResultType != MergeResultType.Increased && mergeResult.ResultType != MergeResultType.New)
            {
                continue;
            }

            string targetDirectory;

            if (mergeResult.ResultType is MergeResultType.Increased)
            {
                File.Delete(mergeResult.Target.Filepath);
                LogInfo($"[Deleted] {mergeResult.Target.Filepath}");
                
                targetDirectory = Path.GetDirectoryName(mergeResult.Target.Filepath)!;
            }
            else
            {
                targetDirectory = newDirectory;
            }
            
            string mergingFilename = Path.GetFileName(mergeResult.Merging.Filepath);
            string newFilepath = Path.Combine(targetDirectory, mergingFilename);
            
            File.Copy(mergeResult.Merging.Filepath, newFilepath, true);
            LogInfo($"[Added] {newFilepath}");
        }
    }

    private List<MergeResult> MergeLibraries(LibrariesMergerConfiguration configuration, IReadOnlyList<Library> libraries1, IReadOnlyList<Library> libraries2)
    {
        List<MergeResult> ret = new();
        
        foreach (Library merging in libraries1)
        {
            bool shouldIgnore = configuration.ToIgnore.Contains(merging.Name);

            if (shouldIgnore)
            {
                ret.Add(new MergeResult(merging, merging, MergeResultType.Ignoring));
                continue;
            }
            
            MergeResult mergeResult = MergeLibrary(merging, libraries2);

            ret.Add(mergeResult);
        }

        return ret;
    }

    private MergeResult MergeLibrary(Library merging, IReadOnlyList<Library> destination)
    {
        foreach (Library checking in destination)
        {
            bool isSameName = merging.Name.Equals(checking.Name);

            if (!isSameName)
            {
                continue;
            }

            MergeResultType resultType = GetVersionMergeResult(ref merging, checking);
            
            return new MergeResult(merging, checking, resultType);
        }

        return new MergeResult(merging, merging, MergeResultType.New);
    }

    private static MergeResultType GetVersionMergeResult(ref Library merging, Library checking)
    {
        for (int i = 0; i < merging.ParsedVersion.Count; i++)
        {
            int mergingVersion = merging.ParsedVersion[i];

            bool checkingHasVersion = checking.ParsedVersion.Count > i;

            if (!checkingHasVersion)
            {
                return MergeResultType.Increased;
            }

            int checkingVersion = checking.ParsedVersion[i];

            if (mergingVersion > checkingVersion)
            {
                return MergeResultType.Increased;
            }

            if (mergingVersion < checkingVersion)
            {
                return MergeResultType.Minor;
            }
        }
        
        return MergeResultType.Same;
    }

    static void LogError(string log)
    {
        Console.WriteLine($"[Error]: {log}");
    }
        
    static void LogInfo(string log)
    {
        Console.WriteLine($"{log}");
    }
        
    static void LogLineJump()
    {
        Console.WriteLine("\n");
    }
        
    List<string> GetAllFilesRecursive(string directory)
    {
        List<string> allFiles = new();
            
        List<string> directoriesToCheck = [directory];

        while (directoriesToCheck.Count > 0)
        {
            string checkingDirectory = directoriesToCheck[0];
            directoriesToCheck.RemoveAt(0);
                
            string[] files = Directory.GetFiles(checkingDirectory);

            foreach (string file in files)
            {
                string fileExtension = Path.GetExtension(file).ToLower();
                bool isValidExtension = ValidExtensions.Contains(fileExtension);

                if (isValidExtension)
                {
                    allFiles.Add(file);
                }
            }
                
            string[] subdirectories = Directory.GetDirectories(checkingDirectory);
            directoriesToCheck.AddRange(subdirectories);
        }

        return allFiles;
    }

    Library ProcessLibrary(string filepath)
    {
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filepath);
        
        string[] parts = fileNameWithoutExtension.Split('-', '.');

        List<int> parsedVersion = new();

        string version = string.Empty;
        
        for (int i = 1; i < parts.Length; i++)
        {
            string partString = parts[i];

            bool couldParse = int.TryParse(partString, out int part);

            if (couldParse)
            {
                if (parsedVersion.Count > 0)
                {
                    version += $".{partString}";
                }
                else
                {
                    version += partString;
                }
                
                parsedVersion.Add(part);
            }
        }
        
        string name = GetLibraryName(filepath);

        return new Library(filepath, name, version, parsedVersion);
    }

    string GetLibraryName(string fileNameWithoutExtension)
    {
        Match match = Regex.Match(fileNameWithoutExtension, @"([^\\\/]+?)(?:-\d+(\.\d+)*(-[^\\\/]+)?)?\.(jar|aar)$");

        if (!match.Success || match.Groups.Count < 1)
        {
            return fileNameWithoutExtension;
        }
        
        return match.Groups[1].Value;
    }

    List<Library> ProcessLibraries(IReadOnlyList<string> libraries)
    {
        List<Library> ret = new();

        foreach (string library in libraries)
        {
            Library processedLibrary = ProcessLibrary(library);
            
            ret.Add(processedLibrary);
        }

        return ret;
    }
}