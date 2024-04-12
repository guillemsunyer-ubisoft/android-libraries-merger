using System.Collections.Generic;

namespace LibsMerger;

public readonly struct Library(string filepath, string name, string version, IReadOnlyList<int> parsedVersion)
{
    public readonly string Filepath = filepath;
    public readonly string Name = name;
    public readonly string Version = version;
    public readonly IReadOnlyList<int> ParsedVersion = parsedVersion;
}