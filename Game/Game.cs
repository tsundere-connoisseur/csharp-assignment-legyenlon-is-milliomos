using System.Diagnostics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace LOIM.Game;

public class Game(QuestionDB questionDB)
{
    [PublicAPI] public const byte       Rounds             = 15;
    [PublicAPI] public const ushort     BaseReward         = 5000;
    [PublicAPI] public const byte       RewardMultiplier   = 2;
    [PublicAPI] public const byte       CheckpointDistance = 5;
    private readonly         QuestionDB questionDB         = questionDB;
    private readonly         State      gameState          = new();

    public void AddPlayer(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("invalid player name", nameof(name));
        if (gameState.ongoing) throw new InvalidOperationException("game has already started");

        gameState.players.Add(new Player(name));
    }

    public void Start()
    {
        gameState.ongoing = true;
        if (!questionDB.TryGetRandomOrderQuestion(out var question))
            throw new ApplicationException("order no questions loaded");
        Ask(question);

        while (gameState.ongoing) Round();
        GameOver();
    }

    private void Round()
    {
        Console.WriteLine($"Round {gameState.currentRound}");

        if (!questionDB.TryGetRandomQuestion(gameState.currentRound, out var question))
            throw new ApplicationException($"no question found for difficulty {gameState.currentRound}");

        Ask(question);
    }

    private void Ask(Question question)
    {
        Console.WriteLine(question.Category);
        Console.WriteLine(question.QuestionText);
        for (var letter = 'A'; letter < question.Answers.Length; letter++)
            Console.WriteLine($"{letter}: {question.Answers[letter - 'A']}");

#if DEBUG
        Console.WriteLine(question.CorrectAnswer);
#endif // DEBUG

        Console.WriteLine($"{gameState.selectedPlayer.Name}'s answer (Q to stop): ");

        char choice;

        while (true)
        {
            choice = Console.ReadKey(true).KeyChar;
            if (!char.IsBetween(choice, 'A', (char)('A' + question.Answers.Length - 1)) && choice != 'Q')
            {
                Console.WriteLine($"Answer must be between 'A' and '{(char)('A' + question.Answers.Length - 1)}'");
                continue;
            }

            break;
        }

        if (choice == 'Q') Quit();
        else if (question.Guess(choice)) CorrectAnswer();
        else WrongAnswer();
    }

    private void WrongAnswer()
    {
        Console.WriteLine("incorrect answer");
        gameState.ongoing   = false;
        gameState.wonAmount = gameState.guearanteedReward;
    }

    private void CorrectAnswer()
    {
        Console.WriteLine("correct answer");
        gameState.wonAmount *= RewardMultiplier;
        Console.WriteLine($"Amount {gameState.wonAmount}");
        if (gameState.currentRound % CheckpointDistance == 0)
        {
            gameState.guearanteedReward = gameState.wonAmount;
            Console.WriteLine($"guaranteed amount: {gameState.guearanteedReward}");
        }

        gameState.currentRound++;
        gameState.ongoing = gameState.currentRound <= Rounds;
    }

    private void Quit()
    {
        Console.WriteLine("quited");
        gameState.ongoing = false;
    }

    private void GameOver()
    {
        Console.WriteLine("Game over");
        Console.WriteLine($"{gameState.selectedPlayer.Name} is going home with {gameState.wonAmount}");
    }

    private void Ask(OrderQuestion question)
    {
        Console.WriteLine(question.Category);
        Console.WriteLine(question.Task);
        for (byte i = 0; i < OrderQuestion.ItemCount; i++)
            Console.WriteLine($"{(char)('A' + i)}. {question.Items[i]}'");

#if DEBUG
        Console.WriteLine(question.Order);
#endif // DEBUG

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

        gameState.selectedPlayer = answerTimes.GetValueAtIndex(0);
        Console.WriteLine($"{gameState.selectedPlayer.Name} was the fastest");
    }

    private class State
    {
        public readonly List<Player> players      = [];
        public          byte         currentRound = 1;
        public          bool         ongoing;
        public          Player       selectedPlayer;
        public          ulong        wonAmount = BaseReward;
        public          ulong        guearanteedReward;
    }
}
