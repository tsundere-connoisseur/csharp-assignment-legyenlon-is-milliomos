using System.Diagnostics;
using JetBrains.Annotations;
using LOIM.Game.Display;
using LOIM.Game.Helpers;

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

    public async Task Run()
    {
        // gameState.ongoing = true;
        // if (!questionDB.TryGetRandomOrderQuestion(out var question))
        //     throw new ApplicationException("order no questions loaded");
        // Ask(question);
        //
        // while (gameState.ongoing) await Round();
        // GameOver();
        gameState.ongoing = true;

        if (!questionDB.TryGetRandomOrderQuestion(out var orderQuestion))
            throw new ApplicationException("order no questions loaded");

        gameState.currentQuestion = orderQuestion;
        var answer = string.Empty;
        var reason = EndReason.Quit;
        var errors = new List<string>();

        var playerTimes = new SortedList<long, Player>();
        var playerIdx   = 0;

        // FIXME: cannpt start task
        async Task AddError(string error, TimeSpan forAmount)
        {
            errors.Add(error);
            await Task.Delay(forAmount);
            errors.Remove(error);
        }

        var sw = Stopwatch.StartNew();

        while (gameState.ongoing)
        {
            if (!gameState.display.IsActive) break;
            gameState.display.MainLoopFrameStart();
            if (!gameState.display.IsActive) break;

            if (playerIdx < gameState.players.Count)
            {
                orderQuestion.Display(gameState.display);
#if DEBUG
                gameState.display.DisplayLine("DEBUG!");
                gameState.display.DisplayLine(orderQuestion.Order.ToString());
#endif
                if (gameState.display.Prompt($"{gameState.players[playerIdx].Name}> ", ref answer))
                {
                    if (gameState.currentQuestion.ValidateAnswer(answer) is { } err)
                    {
                        AddError(err, TimeSpan.FromSeconds(3)).Start();
                    }
                    else if (gameState.currentQuestion.CheckAnswer(answer))
                    {
                        playerTimes.Add(sw.Elapsed.Ticks, gameState.players[playerIdx]);
                        playerIdx++;
                        if (playerIdx == gameState.players.Count)
                        {
                            gameState.selectedPlayer = playerTimes.GetValueAtIndex(0);
                            if (!questionDB.TryGetRandomQuestion(gameState.currentRound, out var firstQuestion))
                                throw new ApplicationException("no questions found");

                            gameState.currentQuestion = firstQuestion;
                        }
                    }
                    
                    answer = string.Empty;
                }

                gameState.display.MainLoopFrameEnd();
                continue;
            }

            gameState.currentQuestion.Display(gameState.display);
            
            #if DEBUG
            gameState.display.DisplayLine("DEBUG!");
            gameState.display.DisplayLine($"{((Question)gameState.currentQuestion).CorrectAnswer}");
            #endif

            if (gameState.currentQuestion is Question currentQuestion)
            {
                gameState.display.DisplayLine("available helps");
                // FIXME: incorrect display values
                gameState.display.DisplayGrid(1, (ulong)gameState.helpers.Count, false,
                                              [..gameState.helpers.Select(it => it.Name)]);
            }

            if (gameState.display.Prompt($"{gameState.selectedPlayer.Name}> ", ref answer))
            {
                if (answer == "q")
                {
                    break;
                }
                else if (int.TryParse(answer, out var helpIdx))
                {
                    if (gameState.currentQuestion is not Question question) continue;
                    if (helpIdx < 0) AddError("help index must be greater than zero", TimeSpan.FromSeconds(3)).Start();
                    else if (helpIdx >= gameState.helpers.Count)
                        AddError("help index is out of range", TimeSpan.FromSeconds(3)).Start();
                    else
                    {
                        gameState.currentQuestion = await gameState.helpers[helpIdx].Help(gameState, question);
                        gameState.helpers.RemoveAt(helpIdx);
                    }
                }
                else if (gameState.currentQuestion.ValidateAnswer(answer) is { } err)
                {
                    AddError(err, TimeSpan.FromSeconds(3)).Start();
                }
                else if (gameState.currentQuestion.CheckAnswer(answer))
                {
                    if (!questionDB.TryGetRandomQuestion(++gameState.currentRound, out var question))
                    {
                        reason = EndReason.Won;
                        break;
                    }

                    gameState.currentQuestion = question;
                }
                else
                {
                    reason = EndReason.Lost;
                    break;
                }
                
                answer =  string.Empty;
            }

            gameState.display.MainLoopFrameEnd();
        }

        GameOver();
    }

    private async Task Round()
    {
        Console.WriteLine($"Round {gameState.currentRound}");

        if (!questionDB.TryGetRandomQuestion(gameState.currentRound, out var question))
            throw new ApplicationException($"no question found for difficulty {gameState.currentRound}");

        await Ask(question);
    }

    private async Task Ask(Question question)
    {
        display_question:
        Console.WriteLine(question.Category);
        Console.WriteLine(question.QuestionText);
        for (var i = 0; i < question.Answers.Length; i++)
            Console.WriteLine($"{(char)('A' + i)}: {question.Answers[i]}");

#if DEBUG
        Console.WriteLine(question.CorrectAnswer);
#endif // DEBUG

        Console.WriteLine("available helps");
        for (byte i = 0; i < gameState.helpers.Count; i++)
            Console.WriteLine($"[{i}]: {gameState.helpers[i].Name}");

        Console.WriteLine($"{gameState.selectedPlayer.Name}'s answer (Q to stop): ");

        char choice;

        while (true)
        {
            choice = Console.ReadKey(true).KeyChar;

            if (char.IsAsciiDigit(choice))
            {
                var value = choice - '0';
                if (value < gameState.helpers.Count)
                {
                    var helper = gameState.helpers[value];
                    Console.WriteLine($"using {helper.Name}");

                    gameState.helpers.Remove(helper);
                    question = await helper.Help(gameState, question);
                    goto display_question;
                }

                Console.WriteLine("helper does not exist");

                continue;
            }

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
        // edge case, quiting in the first round
        if (gameState.wonAmount == BaseReward) gameState.wonAmount = 0;
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

    public class State
    {
        public const    byte          MaxHelperCount = 10; // only able to read 1 char -> 0-9
        public readonly Random        random         = new(DateTime.Now.Nanosecond);
        public          IGameDisplay  display;
        public readonly List<Player>  players = [];
        public readonly List<IHelper> helpers = [];
        public          IQuestion     currentQuestion;
        public          byte          currentRound = 1;
        public          bool          ongoing;
        public          Player        selectedPlayer;
        public          ulong         wonAmount = BaseReward;
        public          ulong         guearanteedReward;
    }
}
