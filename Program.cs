using System.Globalization;
using LOIM.Game;

namespace LOIM;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Console.WriteLine("tet");

        var game = new Game.Game(await QuestionDB.LoadAsync(new(Path.Combine("..", "..", "..", "Data", "kerdes.txt")),
                                                            new(Path.Combine("..", "..", "..", "Data",
                                                                             "sorkerdes.txt"))));

        long       playerCount = 0;
        while (true)
        {
            Console.Write($"Name for player {playerCount + 1} (press enter to begin the game): ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) break;
            game.AddPlayer(input.Trim());
            playerCount++;
        }

        if (playerCount == 0)
        {
            await Console.Error.WriteLineAsync("insufficient number of players");
            return;
        }
        
        game.Start();
    }
}
