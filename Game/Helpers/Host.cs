namespace LOIM.Game.Helpers;

public class Host : IHelper
{
    public string Name => "Host";

    public Task<Question> Help(Game.State gameState, Question question)
    {
        question.Answers[question.CorrectAnswer - 'A'] += " (I think this is the correct one)";

        return Task.FromResult(question);
    }
}
