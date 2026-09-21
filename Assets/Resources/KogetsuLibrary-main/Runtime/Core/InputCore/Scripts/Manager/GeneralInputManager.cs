using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
                string assetPath = "Packages/com.Kogetsu.Library/Runtime/Assets/Prefab/DesignPattern/EventBus.prefab";

                UnityEngine.Object prefab = null;
#if UNITY_EDITOR
                prefab = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);

                Debug.LogError("<color=red>[Error]</color> <color=orange><b>Don't have EventBus in this scene.</b></color>" + Environment.NewLine
                    + "Please create <b>KogetsuLibrary's EventBus</b> in this scene." + Environment.NewLine
                    + $"Path: <u><color=#87CEEB><i>{assetPath}</i></color></u>", prefab);
#endif

                return;
            }

            EventBus.Instance.Publish<InputEvent>(new InputEvent(context));
        }
    }
}