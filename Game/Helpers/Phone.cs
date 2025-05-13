namespace LOIM.Game.Helpers;

public class Phone : IHelper
{
    public string Name => "Phone";

    public async Task<Question> Help(Game.State gameState, Question question)
    {
        question.Answers[question.CorrectAnswer - 'A'] += " (I think this is the correct one)";

        await Task.Delay(TimeSpan.FromSeconds(30));

        return question;
    }
}
