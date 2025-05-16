using LOIM.Game.Display;

namespace LOIM.Game;

public interface IQuestion
{
    /// <summary>
    /// display the question
    /// </summary>
    public void Display(IGameDisplay display);

    /// <summary>
    /// returns whether the submitted answer is correct or not
    /// <remarks>assumes that <see cref="CheckAnswer"/> ran and returned null</remarks>
    /// </summary>
    public bool CheckAnswer(ReadOnlySpan<char> answer);

    /// <summary>
    /// returns a message if the answer is ill-formed
    /// </summary>
    public string? ValidateAnswer(ReadOnlySpan<char> answer);
}
