using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace LOIM.Game;

public class Game(QuestionDB questionDB)
{
    [PublicAPI] public const byte       Rounds     = 15;
    private readonly         QuestionDB questionDB = questionDB;
    private readonly         State      gameState  = new();

    public void AddPlayer(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("invalid player name", nameof(name));
        if (gameState.started) throw new InvalidOperationException("game has already started");

        gameState.players.Add(new Player(name));
    }

    public void Start()
    {
        gameState.started = true;
        if (!questionDB.TryGetRandomOrderQuestion(out var question))
            throw new ApplicationException("order no questions loaded");
        Ask(question);
    }

    private void Ask(OrderQuestion question)
    {
        Console.WriteLine(question.Category);
        Console.WriteLine(question.Task);
        for (byte i = 0; i < OrderQuestion.ItemCount; i++)
            Console.WriteLine($"{(char)('A' + i)}. {question.Items[i]}'");

        // cannot wait for extension properties
        var answerTimes = new SortedList<long, Player>(gameState.players.Count);

        foreach (var player in gameState.players)
        {
            var sw = new Stopwatch();
            sw.Start();
            while (true)
            {
                Console.Write($"{player.Name}'s answer: ");
                var answer = Console.ReadLine();
                if (string.IsNullOrEmpty(answer) || answer.Length != OrderQuestion.ItemCount ||
                    answer.Any(it => !char.IsBetween(it, 'A', (char)('A' + OrderQuestion.ItemCount))))
                {
                    Console.WriteLine("invalid answer. It needs to be 4 characters long and can only contain letters in the range of 'A'..='D'");
                    continue;
                }

                if (question.Guess(OrderQuestion.CharacterOrder.Sequence(answer.AsSpan())))
                    break;

                Console.WriteLine("incorrect");
            }
            // it is very unlikely that 2 players will have the same time
            answerTimes.Add(sw.ElapsedTicks, player);
        }

        gameState.selectedPlayer = answerTimes[0];
        Console.WriteLine($"{gameState.selectedPlayer.Name} was the fastest");
    }

    private class State
    {
        public readonly List<Player> players = [];
        public          byte         currentRound;
        public          bool         started;
        public          Player       selectedPlayer;
    }
}
