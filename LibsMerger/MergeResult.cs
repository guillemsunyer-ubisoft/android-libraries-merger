namespace LibsMerger;

public readonly struct MergeResult(Library merging, Library target, MergeResultType resultType)
{
    public readonly Library Merging = merging;
    public readonly Library Target = target;
    public readonly MergeResultType ResultType = resultType;
}