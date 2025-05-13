namespace LOIM.Game.Helpers;

public class HalveIncorrect : IHelper
{
    public string Name => "Halve";

    public Task<Question> Help(Game.State gameState, Question question)
    {
        List<int> indices = [..Enumerable.Range(0, question.Answers.Length)];
        indices.Remove(question.CorrectAnswer - 'A');
        
        var half = question.Answers.Length / 2;
        for (var i = 0; i < half; i++)
        {
            var randomIndex = Random.Shared.Next(0, indices.Count);
            question.Answers[indices[randomIndex]] = string.Empty;
            indices.RemoveAt(randomIndex);
        }

        return Task.FromResult(question);
    }
}
