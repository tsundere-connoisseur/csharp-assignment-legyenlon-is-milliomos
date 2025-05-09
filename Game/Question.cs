using System.Linq.Expressions;
using JetBrains.Annotations;

namespace LOIM.Game;

public readonly struct Question
{
    [PublicAPI] public const    byte                     MaxLevel      = 15;
    [PublicAPI] public const    byte                     MinLevel      = 1;
    [PublicAPI] public const    byte                     QuestionCount = 4;
    [PublicAPI] public readonly byte                     Difficulty;
    [PublicAPI] public readonly string                   Category;
    [PublicAPI] public readonly string                   QuestionText;
    [PublicAPI] public readonly Dictionary<char, string> Answers;
    [PublicAPI] public readonly char                     CorrectAnswer;

    public static Question Parse(in string line, char delimiter = ';') => new(in line, delimiter);

    private Question(in string repr, char delimiter = ';')
    {
        var src          = repr.AsSpan();
        var dataSegments = src.Split(delimiter);

        EnsureNext(dataSegments);
        ParseData(ref Difficulty, src[dataSegments.Current], src => byte.Parse(src),
                  res => res >= MinLevel && res <= MaxLevel);
    }

    private static void ParseData<T>(ref T dest, ReadOnlySpan<char> src, Func<ReadOnlySpan<char>, T> parser,
                                     Expression<Func<T, bool>> validate)
    {
        var res       = parser(src);
        var validator = validate.Compile();
        if (!validator(res)) throw new FormatException($"Validation failed for value {res} ({validate.Body})");
        dest = res;
    }

    private static void EnsureNext(MemoryExtensions.SpanSplitEnumerator<char> enumerator)
    {
        if (!enumerator.MoveNext()) throw new FormatException("input string does not contain all of the required data");
    }
}
