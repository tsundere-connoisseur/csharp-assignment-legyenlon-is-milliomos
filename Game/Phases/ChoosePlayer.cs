using System.Diagnostics;
using LOIM.Game.Display;

namespace LOIM.Game.Phases;

public class ChoosePlayer : IGamePhase
{
    private          OrderQuestion?           question;
    private          int                      playerIdx;
    private          Stopwatch                sw          = new();
    private readonly SortedList<long, Player> playerTimes = [];
    private          string                   answer      = string.Empty;

    public IGamePhase? Execute(Game.State gameState, QuestionDB questionDB)
    {
        if (this.question is null)
            if (questionDB.TryGetRandomOrderQuestion(out var orderQuestion))
            {
                this.question = orderQuestion;
                sw.Start();
            }
            else
                throw new ApplicationException("no order questions");

        // ReSharper disable once LocalVariableHidesMember
        var question = this.question.Value;

        question.Display(gameState.display);

#if DEBUG
        gameState.display.DisplayLine("DEBUG!");
        gameState.display.DisplayLine(question.Order.ToString());
#endif

        if (!gameState.display.Prompt($"< {gameState.players[playerIdx].Name}", ref answer)) return this;
        
        if (question.ValidateAnswer(answer) is { } err)
        {
            gameState.AddMessage(err, DisplayMessageType.Error);
        }
        else if (question.CheckAnswer(answer))
        {
            playerTimes.Add(sw.Elapsed.Ticks, gameState.players[playerIdx]);
            sw.Restart();
            playerIdx++;
            if (playerIdx == gameState.players.Count)
            {
                gameState.selectedPlayer = playerTimes.GetValueAtIndex(0);
                return new Questions();
            }
        }
        else
        {
            gameState.AddMessage("wrong answer");
        }

        answer = string.Empty;

        return this;
    }
}
