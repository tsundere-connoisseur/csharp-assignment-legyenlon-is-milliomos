using JetBrains.Annotations;

namespace LOIM.Game.Display;

// interface for displaying the game
[PublicAPI]
public interface IGameDisplay
{
    // display a single line of text
    public void         DisplayLine(string    line);
    public void         DisplayGrid(ulong     rows, ulong columns, params string[] gridItems);
    public Task<string> Prompt(string         promptText);
    public void         DisplayMessage(string message, DisplayMessageType type);

    public void MainLoopFrameStart();
    public void MainLoopFrameEnd();
    
    public bool IsActive { get; }
}
