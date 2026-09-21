using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    [RequireComponent(typeof(PlayerInput))]
    [CreateHierarchyMenu("KogetsuLibrary/Core/Manager")]
    public class BasicInputManager : MonoBehaviour, IAbility
    {
        [Header("ObserverChannels")]
        [Required]
        [SerializeField] protected BasicMovementInputObserverSO BasicObserverChannel;

        protected void Awake()
        {
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        protected void OnDestroy()
        {
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneUnloaded(Scene scene)
        {
            BasicObserverChannel?.ClearAllChannels();
        }

        public void OnMoveInput(InputAction.CallbackContext context)
        {
            if (BasicObserverChannel == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("MoveInputChannelSO is not Assign");
#endif
                return;
            }

            BasicObserverChannel.SendMoveSignal(context.ReadValue<Vector3>());
        }

        public void OnJumpInput(InputAction.CallbackContext context)
        {
            if (BasicObserverChannel == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("JumpEventChannelSO is not Assign");
#endif
                return;
            }

            if (context.performed)  BasicObserverChannel.SendJumpSignal();
            if (context.canceled)   BasicObserverChannel.SendJumpReleasedSignal();
        }

        public void OnInteractionInput(InputAction.CallbackContext context)
        {
            if (BasicObserverChannel == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("InteractionInputSO is not Assign");
#endif
                return;
            }

            if (!context.performed) return;

            BasicObserverChannel.SendInteractionSignal();
        }

        public void OnLeftClickInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;

            Vector2 clickPos = Mouse.current.position.ReadValue();
            Debug.Log($"Left Click Pos : {clickPos}");
            BasicObserverChannel.SendLeftClickSignal(new ClickData(clickPos));
        }

        public void OnMiddelClickInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;

            Vector2 clickPos = Mouse.current.position.ReadValue();
            Debug.Log($"Middel Click Pos : {clickPos}");
            BasicObserverChannel.SendMiddleClickSignal(new ClickData(clickPos));
        }

        public void OnRightClickInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;
            Vector2 clickPos = Mouse.current.position.ReadValue();
            Debug.Log($"Right Click Pos : {clickPos}");
            BasicObserverChannel.SendRightClickSignal(new ClickData(clickPos));
        }

        // ─── Player Actions ───────────────────────────────────────────────
        public void OnRollInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;
            BasicObserverChannel.SendRollSignal();
        }

        public void OnFlashlightInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;
            BasicObserverChannel.SendFlashlightToggleSignal();
        }

        public void OnShootInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;
            BasicObserverChannel.SendShootSignal();
        }

        /// <summary>Map action type: Value / Vector2 — ส่ง screen position ทุก frame ที่เมาส์ขยับ</summary>
        public void OnMousePositionInput(InputAction.CallbackContext context)
        {
            if (BasicObserverChannel == null) return;
            BasicObserverChannel.SendMousePositionSignal(context.ReadValue<Vector2>());
        }

        /// <summary>Map action type: Button — กด R เพื่อ restart scene</summary>
        public void OnRestartInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;
            BasicObserverChannel.SendRestartSignal();
        }
    }
}
