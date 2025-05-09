using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace LOIM.Game;

public sealed class QuestionDB
{
    private readonly List<OrderQuestion>                    orderQuestions               = [];
    private readonly SortedDictionary<long, List<Question>> questionsOrderedByDifficulty = [];

    [PublicAPI]
    public bool TryGetRandomQuestion(long difficulty, out Question question)
    {
        question = default;
        if (!questionsOrderedByDifficulty.TryGetValue(difficulty, out var questions)) return false;

        question = questions[Random.Shared.Next(0, questions.Count)];
        return true;
    }

    [PublicAPI]
    public bool TryGetRandomOrderQuestion(out OrderQuestion orderQuestion)
    {
        orderQuestion = default;
        if (orderQuestions.Count == 0) return false;
        orderQuestion = orderQuestions[Random.Shared.Next(0, orderQuestions.Count)];
        return true;
    }

    [PublicAPI]
    public static async Task<QuestionDB> LoadAsync(FileInfo questions, FileInfo orderQuestions)
    {
        Debug.Assert(questions.Exists, $"{nameof(questions.Exists)} ({questions.FullName})");
        Debug.Assert(orderQuestions.Exists, $"{nameof(orderQuestions.Exists)} ({orderQuestions.FullName})");

        var questionDB = new QuestionDB();

        using var questionsStream = questions.OpenText();
        while (true)
        {
            var line = await questionsStream.ReadLineAsync();
            if (line is null) break;
            line = line.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;
            var question = Question.Parse(in line);

            if (!questionDB.questionsOrderedByDifficulty.TryGetValue(question.Difficulty, out var questionList))
            {
                questionList = [question];
                questionDB.questionsOrderedByDifficulty.Add(question.Difficulty, questionList);
            }
            else
            {
                questionList.Add(question);
            }
        }

        using var orderQuestionsStream = orderQuestions.OpenText();
        while (true)
        {
            var line = await orderQuestionsStream.ReadLineAsync();
            if (line is null) break;
            line = line.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;
            questionDB.orderQuestions.Add(OrderQuestion.Parse(in line));
        }

        return questionDB;
    }
}
