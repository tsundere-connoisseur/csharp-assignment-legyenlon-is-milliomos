using System.Linq.Expressions;
using JetBrains.Annotations;
using LOIM.Game.Display;
using LOIM.Util;

namespace LOIM.Game;

public readonly struct Question : IQuestion
{
    [PublicAPI] public const    byte     MaxLevel      = 15;
    [PublicAPI] public const    byte     MinLevel      = 1;
    [PublicAPI] public const    byte     QuestionCount = 4;
    [PublicAPI] public readonly byte     Difficulty;
    [PublicAPI] public readonly string   Category;
    [PublicAPI] public readonly string   QuestionText;
    [PublicAPI] public readonly string[] Answers = new string[QuestionCount];
    [PublicAPI] public readonly char     CorrectAnswer;

    [PublicAPI]
    public static Question Parse(in string line, char delimiter = ';') => new(in line, delimiter);

    /// <summary>
    /// returns whether the input character is the correct answer or not
    /// <param name="guess">value in the A..=D range</param>
    /// </summary>
    [PublicAPI]
    public bool Guess([ValueRange('A', 'A' + QuestionCount)] char guess) => guess == CorrectAnswer;

    private Question(in string repr, char delimiter = ';')
    {
        var src          = repr.AsSpan();
        var dataSegments = src.Split(delimiter);

        dataSegments.EnsureNext();
        ParseData(ref Difficulty, src[dataSegments.Current], src => byte.Parse(src),
                  res => res >= MinLevel && res <= MaxLevel);

        dataSegments.EnsureNext();
        QuestionText = src[dataSegments.Current].ToString();

        for (byte i = 0; i < QuestionCount; i++)
        {
            dataSegments.EnsureNext();
            Answers[i] = src[dataSegments.Current].ToString();
        }

        dataSegments.EnsureNext();
        ParseData(ref CorrectAnswer, src[dataSegments.Current], src => char.ToUpper(src[0]),
                  res => char.IsBetween(res, 'A', 'D'));

        dataSegments.EnsureNext();
        Category = src[dataSegments.Current].ToString();
    }

    private static void ParseData<T>(ref T dest, ReadOnlySpan<char> src, Func<ReadOnlySpan<char>, T> parser,
                                     Expression<Func<T, bool>> validate)
    {
        var res       = parser(src);
        var validator = validate.Compile();
        if (!validator(res)) throw new FormatException($"Validation failed for value {res} ({validate.Body})");
        dest = res;
    }

    public void Display(IGameDisplay display)
    {
        display.DisplayLine(Category);
        display.DisplayLine(QuestionText);
        display.DisplayGrid(2, 2, Answers);
    }

    public bool CheckAnswer(ReadOnlySpan<char> answer) => answer[0] == CorrectAnswer;

    public string? ValidateAnswer(ReadOnlySpan<char> answer)
    {
        return !answer.ValidateAnswer(1)
            ? $"answer must be {1} characters long and must only contain characters between 'A' and {(char)('A' + Answers.Length - 1)}"
            : null;
    }
}
