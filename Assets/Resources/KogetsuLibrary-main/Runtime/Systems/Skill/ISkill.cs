using UnityEngine.InputSystem;

namespace Kogetsu.Library.Core
{
    public interface ISkill
    {
        ushort Id { get; }
        string Name { get; }
        string Tag { get; }
        bool IsReady { get; }
        void Tick(float deltaTime);
    }
}