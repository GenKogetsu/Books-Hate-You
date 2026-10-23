using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library
{
    public record struct InputEvent(InputAction.CallbackContext Context) : IEvent;

}
