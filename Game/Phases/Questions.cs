using LOIM.Game.Display;

namespace LOIM.Game.Phases;

public class Questions : IGamePhase
{
    private byte      currentRound = 1;
    private Question? question;
    private string    answer = string.Empty;

    public IGamePhase? Execute(Game.State gameState, QuestionDB questionDB)
    {
        if (this.question is null)
        {
            if (questionDB.TryGetRandomQuestion(currentRound, out var currentQuestion))
            {
                this.question = currentQuestion;
            }
            else
            {
                gameState.reason = EndReason.Won;
                return null;
            }
        }

        // ReSharper disable once LocalVariableHidesMember
        var question = this.question.Value;

        question.Display(gameState.display);

#if DEBUG
        gameState.display.DisplayLine("DEBUG!");
        gameState.display.DisplayLine($"{question.CorrectAnswer}");
#endif

        gameState.display.DisplayLine($"round {currentRound}");
        gameState.display.DisplayLine($"won: {gameState.wonAmount}");
        gameState.display.DisplayLine($"guaranteed: {gameState.guearanteedReward}");
        
        gameState.display.DisplayLine("available helps");
        // FIXME: incorrect display values
        gameState.display.DisplayGrid(1, (ulong)gameState.helpers.Count, false,
                                      [..gameState.helpers.Select(it => it.Name)]);

        if (!gameState.display.Prompt($"{gameState.selectedPlayer.Name}> ", ref answer)) return this;
        if (answer == "Q")
        {
            gameState.reason = EndReason.Quit;
            return null;
        }
        else if (int.TryParse(answer, out var helpIdx))
        {
            if (helpIdx < 0) gameState.AddMessage("help index must be greater than zero", DisplayMessageType.Error);
            else if (helpIdx >= gameState.helpers.Count)
                gameState.AddMessage("help index is out of range", DisplayMessageType.Error);
            else
            {
                question      = gameState.helpers[helpIdx].Help(gameState, question);
                this.question = question;
                gameState.helpers.RemoveAt(helpIdx);

                return this;
            }
        }
        else if (question.ValidateAnswer(answer) is { } err)
            gameState.AddMessage(err, DisplayMessageType.Error);
        else if (question.CheckAnswer(answer))
        {
            gameState.wonAmount *= 2;
            if (gameState.wonAmount == 0) gameState.wonAmount = 10000;
            if (currentRound % 5 == 0) gameState.guearanteedReward = gameState.wonAmount;

            if (questionDB.TryGetRandomQuestion(++currentRound, out var nextQuestion))
                this.question = nextQuestion;
            else
            {
                gameState.reason = EndReason.Won;
                return null;
            }
        }
        else
        {
            gameState.wonAmount = gameState.guearanteedReward;
            gameState.reason    = EndReason.Lost;
            return null;
        }

        answer = string.Empty;

        return this;
    }
}
