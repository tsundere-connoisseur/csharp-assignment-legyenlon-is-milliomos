using System.Globalization;
using LOIM.Game;
using LOIM.Game.Display;
using LOIM.Game.Helpers;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace LOIM;

internal static class Program
{
    public static async Task Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Console.WriteLine("tet");

        VeldridStartup.CreateWindowAndGraphicsDevice(new WindowCreateInfo(100, 100, 1280, 720, WindowState.Normal,
                                                                          "legyen ön is milliomos"),
                                                     out var window, out var gd);
        
        window.Resized += () =>
                           {
                               gd.MainSwapchain.Resize((uint)window.Width, (uint)window.Height);
                           };

        var game = new Game.Game(await QuestionDB.LoadAsync(new(Path.Combine("..", "..", "..", "Data", "kerdes.txt")),
                                                            new(Path.Combine("..", "..", "..", "Data",
                                                                             "sorkerdes.txt"))))
                  .AddHelp(new HalveIncorrect())
                  .AddHelp(new Audience())
                  .AddHelp(new Phone())
                  .AddHelp(new Host())
                  .WithDisplay(new ImGuiDisplay(window, gd));

        long playerCount = 0;
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

        game.Run();
    }
}
