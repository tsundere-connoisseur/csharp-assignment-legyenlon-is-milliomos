using System.Diagnostics;
using JetBrains.Annotations;
using LOIM.Game.Display;
using LOIM.Game.Helpers;
using LOIM.Game.Phases;

namespace LOIM.Game;

public class Game(QuestionDB questionDB)
{
    [PublicAPI] public const byte       Rounds             = 15;
    [PublicAPI] public const byte       RewardMultiplier   = 2;
    [PublicAPI] public const byte       CheckpointDistance = 5;
    private readonly         QuestionDB questionDB         = questionDB;
    private readonly         State      gameState          = new();

    public void AddPlayer(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("invalid player name", nameof(name));
        if (gameState.ongoing) throw new InvalidOperationException("game has already started");
        if (gameState.helpers.Count == State.MaxHelperCount)
            throw new InvalidOperationException("max amount of helpers reached");

        gameState.players.Add(new Player(name));
    }

    public Game AddHelp(IHelper helper)
    {
        ArgumentNullException.ThrowIfNull(helper);
        if (gameState.ongoing) throw new InvalidOperationException("game has already started");
        gameState.helpers.Add(helper);
        return this;
    }

    public Game WithDisplay(IGameDisplay display)
    {
        gameState.display = display;
        return this;
    }

    public void Run()
    {
        gameState.ongoing = true;

        IGamePhase phase = new ChoosePlayer();

        while (gameState.ongoing)
        {
            if (!gameState.display.IsActive) break;
            gameState.display.MainLoopFrameStart();
            if (!gameState.display.IsActive) break;

            gameState.now = (ulong)DateTime.Now.Ticks;

            if (phase.Execute(gameState, questionDB) is { } nextPhase) phase = nextPhase;
            else gameState.ongoing                                           = false;

            gameState.DisplayMessages();

            gameState.display.MainLoopFrameEnd();
        }

        Console.WriteLine($"Game over ({gameState.reason})");
        Console.WriteLine($"{gameState.selectedPlayer.Name} is going home with {gameState.wonAmount}");
    }

    public class State
    {
        public const     byte MaxHelperCount = 10; // only able to read 1 char -> 0-9
        public readonly  Random random = new(DateTime.Now.Nanosecond);
        public           IGameDisplay display;
        public readonly  List<Player> players = [];
        public readonly  List<IHelper> helpers = [];
        public           bool ongoing;
        public           EndReason reason;
        public           Player selectedPlayer;
        public           ulong wonAmount;
        public           ulong guearanteedReward;
        public           ulong now;
        private readonly List<(ulong removeAt, DisplayMessageType type, string message)> messages = [];

        public void AddMessage(string    message, DisplayMessageType type = DisplayMessageType.DontCare,
                               TimeSpan? duration = null)
        {
            messages.Add(((ulong)(duration ?? TimeSpan.FromSeconds(3)).Ticks + now, type, message));
        }

        public void DisplayMessages()
        {
            for (var i = 0; i < messages.Count; i++)
            {
                var message = messages[i];
                if (message.removeAt <= now)
                {
                    messages.RemoveAt(i);
                    i--;
                    continue;
                }

                display.DisplayMessage(message.message, message.type);
            }
        }
    }
}
