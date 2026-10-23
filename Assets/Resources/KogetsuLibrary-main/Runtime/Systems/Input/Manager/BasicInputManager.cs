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

            SendClick("Left", BasicObserverChannel.SendLeftClickSignal);
        }

        public void OnMiddelClickInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;

            SendClick("Middel", BasicObserverChannel.SendMiddleClickSignal);
        }

        public void OnRightClickInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;
            SendClick("Right", BasicObserverChannel.SendRightClickSignal);
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : อ่านตำแหน่งเมาส์ครั้งเดียว เขียน log ตามปุ่ม แล้วส่ง ClickData ผ่าน callback; ผู้เรียกต้องตรวจ input phase และ observer ก่อน </para>
        /// <para> (EN) : Reads the mouse position once, logs the button, then publishes ClickData through the callback; callers check input phase and observer availability. </para>
        /// </summary>
        #endregion
        private static void SendClick(string button, System.Action<ClickData> publish)
        {
            Vector2 clickPos = Mouse.current.position.ReadValue();
            Debug.Log($"{button} Click Pos : {clickPos}");
            publish(new ClickData(clickPos));
        }

        #region Documentation - Notes
        // ─── Player Actions ───────────────────────────────────────────────
        #endregion
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

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : Map action type: Value / Vector2 — ส่ง screen position ทุก frame ที่เมาส์ขยับ </para>
        /// <para> (EN) : Reads a Value/Vector2 input action and sends screen position whenever the mouse moves. </para>
        /// </summary>
        #endregion
        public void OnMousePositionInput(InputAction.CallbackContext context)
        {
            if (BasicObserverChannel == null) return;
            BasicObserverChannel.SendMousePositionSignal(context.ReadValue<Vector2>());
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : Map action type: Button — กด R เพื่อ restart scene </para>
        /// <para> (EN) : Forwards a Button action, conventionally R, to request a scene restart. </para>
        /// </summary>
        #endregion
        public void OnRestartInput(InputAction.CallbackContext context)
        {
            if (!context.performed || BasicObserverChannel == null) return;
            BasicObserverChannel.SendRestartSignal();
        }
    }
}
