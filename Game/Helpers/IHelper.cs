namespace LOIM.Game.Helpers;

public interface IHelper
{
    public       string Name { get; }
    public Task<Question> Help(Game.State gameState, Question question);
}
