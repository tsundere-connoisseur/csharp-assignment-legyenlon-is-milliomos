using System.Globalization;

namespace LOIM.Game.Helpers;

public class Audience : IHelper
{
    public string Name => "Audience";
    
    public Question Help(Game.State gameState, Question question)
    {
        var chocies = new float[question.Answers.Length];
        var acc     = 0f;

        for (var i = 0; i < chocies.Length; i++)
        {
            var v = Random.Shared.NextSingle();
            acc                                            += v;
            chocies[Random.Shared.Next(0, chocies.Length)] += v;
            
            if (i % chocies.Length != 0) continue;
            chocies[question.CorrectAnswer - 'A']++;
            acc++;
        }
        
        foreach (var (str, idx) in chocies.Select((it, idx) => ($"{it/acc:P}", idx)))
        {
            question.Answers[idx] = str;
        }

        return question;
    }
}
