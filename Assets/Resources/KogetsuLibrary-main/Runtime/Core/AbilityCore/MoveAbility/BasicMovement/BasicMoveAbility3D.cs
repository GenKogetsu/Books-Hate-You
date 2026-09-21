using System;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    [Serializable]
    public class BasicMoveAbility3D : BaseMoveAbility3D
    {
        [Header("Movement Settings")]
        [SerializeField] protected bool EnableRotation = true;
        [SerializeField] protected float RotationSpeed = 15f;

        [Tooltip("ถ้าเปิด จะใช้ input เป็น world-space ตรงๆ ไม่ผ่าน camera transform\n" +
                 "ใช้เมื่อ AI ส่ง direction มาเป็น world-space โดยตรง")]
        [SerializeField] protected bool BypassCameraTransform = false;

        [Header("Ball Settings (Physics Rolling)")]
        [SerializeField] protected bool IsBallRolling = false;
        [SerializeField] protected float RollTorque = 10f;

        protected override void OnEnter()
        {
            if (Context.InputObserverChannel == null) return;
            Context.InputObserverChannel.OnMoveChannel += SetInput;
            Context.InputObserverChannel.OnJumpChannel += ExecuteJump;
        }

        protected override void OnExit()
        {
            if (Context.InputObserverChannel == null) return;
            Context.InputObserverChannel.OnMoveChannel -= SetInput;
            Context.InputObserverChannel.OnJumpChannel -= ExecuteJump;
        }

        protected override void OnFixedUpdate()
        {
            if (Context?.Stats == null || Context.Rb == null) return;

            Vector2 clamped = Vector2.ClampMagnitude(CurrentInput, 1f);
            Vector3 rawDir = new(clamped.x, 0f, clamped.y);
            Context.CurrentDirection = Context.SnapDirection(rawDir);

            Vector3 moveDir = Vector3.zero;
            if (CurrentInput.sqrMagnitude > 0.001f)
            {
                if (BypassCameraTransform)
                {
                    moveDir = new Vector3(CurrentInput.x, 0f, CurrentInput.z).normalized;
                }
                else
                {
                    Transform cam = Camera.main.transform;
                    Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
                    Vector3 camRight   = Vector3.Scale(cam.right,   new Vector3(1, 0, 1)).normalized;
                    moveDir = (camForward * CurrentInput.z + camRight * CurrentInput.x).normalized;
                }
            }

            Vector3 targetVel = moveDir * Context.Stats.GetMoveSpeed();
            Context.Rb.linearVelocity = new Vector3(targetVel.x, Context.Rb.linearVelocity.y, targetVel.z);

            if (moveDir == Vector3.zero) return;

            if (IsBallRolling)
            {
                Context.Rb.AddTorque(Vector3.Cross(Vector3.up, moveDir) * RollTorque, ForceMode.Force);
            }
            else if (EnableRotation)
            {
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                Context.Rb.MoveRotation(Quaternion.Slerp(Context.Rb.rotation, targetRot, Time.fixedDeltaTime * RotationSpeed));
            }
        }

        protected virtual void ExecuteJump()
        {
            if (Context?.Stats != null)
                Context.Rb.AddForce(Vector3.up * Context.Stats.GetJumpForce(), ForceMode.Impulse);
        }
    }
}
