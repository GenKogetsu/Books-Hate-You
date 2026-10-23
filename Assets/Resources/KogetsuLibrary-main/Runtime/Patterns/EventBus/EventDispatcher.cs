using System;
using System.Collections.Generic;

namespace Kogetsu.Library.DesignPatternCore
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ตัวกลางส่งเหตุการณ์แบบระบุชนิด เก็บ delegate แยกตามชนิด T โดยไม่อ้างอิง GameObject หรือวงจรชีวิตของ Scene </para>
    /// <para> (EN) : Typed event mediator that stores delegates by T without depending on GameObjects or scene lifecycle. </para>
    /// <para> Refactor : </para>
    /// <para> (TH) : แยกกลไกออกจาก EventBus เพื่อใช้ผ่าน IEventBus และทดสอบได้โดยไม่สร้าง Singleton; listener ที่ลงทะเบียนซ้ำต้องยกเลิกทีละครั้ง </para>
    /// <para> (EN) : Extracted from EventBus for use through IEventBus and testing without a Singleton; repeated registrations require balanced unsubscriptions. </para>
    /// <para> Notes : </para>
    /// <para> (TH) : เรียก listener แบบ synchronous ตามลำดับลงทะเบียน; exception จาก listener จะส่งกลับไปยังผู้เรียกและหยุดการส่งครั้งนั้น </para>
    /// <para> (EN) : Listeners execute synchronously in registration order; a listener exception propagates to the caller and interrupts that dispatch. </para>
    /// </summary>
    #endregion
    public sealed class EventDispatcher : IEventBus
    {
        private readonly Dictionary<Type, Delegate> _listeners = new();

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : เพิ่ม listener หนึ่งรายการให้เหตุการณ์ชนิด T; ข้ามค่า null และอนุญาตให้ delegate เดียวลงทะเบียนหลายครั้ง </para>
        /// <para> (EN) : Adds one listener for T, ignores null, and permits repeated registration of the same delegate. </para>
        /// </summary>
        /// <typeparam name="T">ชนิดเหตุการณ์ที่ใช้เป็น key / Event type used as the dispatch key.</typeparam>
        /// <param name="listener">callback ที่จะรับข้อมูลชนิด T / Callback receiving T.</param>
        #endregion
        public void Subscribe<T>(Action<T> listener) where T : IEvent
        {
            if (listener == null) return;
            _listeners.TryGetValue(typeof(T), out var current);
            _listeners[typeof(T)] = Delegate.Combine(current, listener);
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ยกเลิก listener ที่ตรงกันรายการสุดท้ายหนึ่งรายการ และลบ key เมื่อไม่เหลือผู้รับ; การยกเลิก listener ที่ไม่มีอยู่ไม่มีผล </para>
        /// <para> (EN) : Removes the last matching listener once and removes the key when empty; absent listeners are a no-op. </para>
        /// </summary>
        #endregion
        public void Unsubscribe<T>(Action<T> listener) where T : IEvent
        {
            if (listener == null || !_listeners.TryGetValue(typeof(T), out var current)) return;
            var remaining = Delegate.Remove(current, listener);
            if (remaining == null) _listeners.Remove(typeof(T));
            else _listeners[typeof(T)] = remaining;
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ส่งข้อมูลให้ listener ที่ลงทะเบียนด้วยชนิด T ตรงกันเท่านั้น; การเปลี่ยนรายการระหว่าง callback มีผลกับการ Publish ครั้งถัดไป </para>
        /// <para> (EN) : Dispatches only to listeners registered for exactly T; subscription changes during callbacks affect subsequent publications. </para>
        /// </summary>
        /// <param name="eventData">ข้อมูลเหตุการณ์; key ใช้ typeof(T) ไม่ใช่ชนิด runtime / Event payload; the key is typeof(T), not the runtime type.</param>
        #endregion
        public void Publish<T>(T eventData) where T : IEvent
        {
            if (_listeners.TryGetValue(typeof(T), out var listeners))
                ((Action<T>)listeners)?.Invoke(eventData);
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ล้าง listener ทุกชนิดออกจาก dispatcher ตัวนี้ ไม่ได้ล้าง EventBus หรือ dispatcher ตัวอื่น </para>
        /// <para> (EN) : Clears all listeners owned by this dispatcher, leaving other dispatcher instances untouched. </para>
        /// </summary>
        #endregion
        public void Clear() => _listeners.Clear();
    }
}
