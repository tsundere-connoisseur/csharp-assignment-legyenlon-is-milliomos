namespace LOIM.Game.Phases;

public interface IGamePhase
{
    public IGamePhase? Execute(Game.State gameState, QuestionDB questionDB);
}
