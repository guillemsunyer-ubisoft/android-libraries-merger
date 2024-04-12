namespace LibsMerger;

public readonly struct DuplicatedLibrariesResult(Library library1, Library library2)
{
    public readonly Library Library1 = library1;
    public readonly Library Library2 = library2;
}