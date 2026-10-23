using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    [RequireComponent(typeof(PlayerInput))]
    [CreateHierarchyMenu("KogetsuLibrary/Core/Manager")]
    public class GeneralInputManager : Singleton<GeneralInputManager>
    {
        public void OnPlayerInput(InputAction.CallbackContext context)
        {
            if (EventBus.Instance == null)
            {
#if UNITY_EDITOR
                Debug.LogError("<color=red>[Error]</color> <color=orange><b>Don't have EventBus in this scene.</b></color>" + Environment.NewLine
                    + "Please create <b>KogetsuLibrary's EventBus</b> in this scene.", this);
#endif

                return;
            }

            EventBus.Instance.Publish<InputEvent>(new InputEvent(context));
        }
    }
}
