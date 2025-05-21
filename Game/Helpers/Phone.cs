namespace LOIM.Game.Helpers;

public class Phone : IHelper
{
    public string Name => "Phone";

    public Question Help(Game.State gameState, Question question)
    {
        question.Answers[question.CorrectAnswer - 'A'] += " (I think this is the correct one)";

        return question;
    }
}
