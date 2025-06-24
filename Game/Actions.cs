using System.Numerics;

namespace Game
{
    public interface IAction { }

    public record MoveAction(Vector2 Target) : IAction;

    public record WaitAction(int Duration) : IAction;

    public record SayAction(string Message) : IAction;
}
