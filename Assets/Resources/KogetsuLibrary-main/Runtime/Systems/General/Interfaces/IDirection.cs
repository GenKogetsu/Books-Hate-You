namespace Kogetsu.Library.Core;

public interface IDirection
{
    Vector3 CurrentDiraction { get; }

    Vector3 LastMoveDiraction { get; }

    bool IsJumping { get; }

    bool IsCrouching { get; }
}