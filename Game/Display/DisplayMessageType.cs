using JetBrains.Annotations;

namespace LOIM.Game.Display;

[PublicAPI]
public enum DisplayMessageType
{
    Message,
    Error,
    DontCare = Message,
}
