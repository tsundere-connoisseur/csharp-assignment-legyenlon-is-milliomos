namespace LOIM.Util;

public static class CommonExtensions
{
    public static void EnsureNext(this ref MemoryExtensions.SpanSplitEnumerator<char> enumerator)
    {
        if (!enumerator.MoveNext()) throw new FormatException("input string does not contain all of the required data");
    }
}
