using System;
using Kogetsu.Library.Attribute;

namespace Kogetsu.Library.DesignPatternCore
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ระบบจัดการ Event ส่วนกลางที่ใช้ IEvent เป็นตัวกลางในการสื่อสารแบบ Decoupled </para>
    /// <para> (EN) : Centralized event system using IEvent for decoupled communication. </para>
    /// </summary>
    #endregion
    [CreateHierarchyMenu("KogetsuLibrary/DesignPattern")]
    public class EventBus : Singleton<EventBus>, IEventBus
    {
        #region Fields

        private readonly EventDispatcher _dispatcher = new();

        #endregion // Fields

        #region Public Methods

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ลงทะเบียนเพื่อรับฟังเหตุการณ์ (Subscribe) </para>
        /// <para> (EN) : Register a listener (Subscribe). </para>
        /// </summary>
        #endregion
        public void Subscribe<T>(Action<T> listener) where T : IEvent
        {
            Type type = typeof(T);
            _dispatcher.Subscribe(listener);

#if UNITY_EDITOR
            Debug.Log($"<b><color=#4FC3F7>[EventBus]</color></b> " +
                $"Subscribed to: " +
                $"<color=#81C784>{type.Name}</color>");
#endif
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ยกเลิกการรับฟังเหตุการณ์ (Unsubscribe) </para>
        /// <para> (EN) : Unregister a listener (Unsubscribe). </para>
        /// </summary>
        #endregion
        public void Unsubscribe<T>(Action<T> listener) where T : IEvent
        {
            Type type = typeof(T);
            _dispatcher.Unsubscribe(listener);

#if UNITY_EDITOR
            Debug.Log($"<b><color=#4FC3F7>[EventBus]</color></b> " +
                $"Unsubscribed from: " +
                $"<color=#E57373>{type.Name}</color>");
#endif
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : กระจายสัญญาณเหตุการณ์ไปยังทุกคนที่ Subscribe ไว้ </para>
        /// <para> (EN) : Broadcast an event to all subscribed listeners. </para>
        /// </summary>
        #endregion
        public void Publish<T>(T eventData) where T : IEvent
        {
            Type type = typeof(T);
            _dispatcher.Publish(eventData);

#if UNITY_EDITOR
            Debug.Log($"<b><color=#4FC3F7>[EventBus]</color></b> " +
                $"Published: " +
                $"<color=#FFF176>{type.Name}</color>");
#endif
        }

        #endregion // Public Methods
    }
}

