using System;

namespace Kogetsu.Library.DesignPatternCore
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : จัดการ state ปัจจุบันและส่งต่อ lifecycle ตาม interface ที่แต่ละ state รองรับ; ผู้เรียกต้องสร้าง state และ Initialize context ให้ก่อน </para>
    /// <para> (EN) : Manages the current state and forwards lifecycle calls according to implemented interfaces; callers create states and initialize their contexts. </para>
    /// </summary>
    #endregion
    [Serializable]
    public class StateMachine<TContext>
    {
        public BaseState<TContext> CurrentState { get; private set; }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : เรียก OnExit ของ state เดิม กำหนด state ใหม่ แล้วเรียก OnEnter ตามลำดับ; อนุญาต null และการเข้า state เดิมซ้ำ </para>
        /// <para> (EN) : Calls the old state's OnExit, assigns the new state, then calls OnEnter; null and reentering the same state are supported. </para>
        /// </summary>
        /// <param name="newState">state ที่เตรียม context แล้ว หรือ null เพื่อหยุด / Initialized state, or null to stop dispatching.</param>
        #endregion
        public void ChangeState(BaseState<TContext> newState)
        {
            if (CurrentState is IExitState exitState) exitState.OnExit();
            CurrentState = newState;
            if (CurrentState is IEnterState enterState) enterState.OnEnter();
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ส่ง Update ให้ state ที่รองรับ IUpdateState; ถ้าไม่มี state หรือไม่รองรับจะข้าม </para>
        /// <para> (EN) : Forwards Update to IUpdateState; absent or incompatible states are ignored. </para>
        /// </summary>
        #endregion
        public void Update() 
        { 
            if (CurrentState is IUpdateState updateState) updateState.OnUpdate(); 
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ส่ง physics tick ให้ state ที่รองรับ IFixedUpdateState; controller เป็นผู้เรียกเมธอดนี้ </para>
        /// <para> (EN) : Forwards physics ticks to IFixedUpdateState; the owning controller drives this method. </para>
        /// </summary>
        #endregion
        public void FixedUpdate() 
        { 
            if (CurrentState is IFixedUpdateState fixedUpdateState) fixedUpdateState.OnFixedUpdate(); 
        }
    }
}
