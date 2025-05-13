namespace LOIM.Game.Helpers;

public interface IHelper
{
    public string Name { get; }
    public Question Help(Game.State gameState, Question question);
}
