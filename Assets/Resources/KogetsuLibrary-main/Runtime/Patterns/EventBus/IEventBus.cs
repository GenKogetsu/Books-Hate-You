using System;

namespace Kogetsu.Library.DesignPatternCore
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : สัญญาการส่งเหตุการณ์ร่วมกันระหว่างผู้ส่งและผู้รับ เพื่อลดการผูกกับ implementation </para>
    /// <para> (EN) : Messaging contract shared by publishers and subscribers. </para>
    /// </summary>
    #endregion
    public interface IEventBus
    {
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : เพิ่ม callback สำหรับ T; EventDispatcher รองรับการลงทะเบียน delegate เดิมหลายครั้ง </para>
        /// <para> (EN) : Registers a callback for T; EventDispatcher permits repeated registration of the same delegate. </para>
        /// </summary>
        #endregion
        void Subscribe<T>(Action<T> listener) where T : IEvent;
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ยกเลิก callback หนึ่งรายการของ T โดยใช้ delegate ที่ตรงกับตอน Subscribe </para>
        /// <para> (EN) : Removes one callback for T using a delegate matching the original subscription. </para>
        /// </summary>
        #endregion
        void Unsubscribe<T>(Action<T> listener) where T : IEvent;
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ส่ง payload ของ T ไปยังผู้รับที่ลงทะเบียนผ่าน implementation นี้ </para>
        /// <para> (EN) : Dispatches a T payload to listeners registered with this implementation. </para>
        /// </summary>
        #endregion
        void Publish<T>(T eventData) where T : IEvent;
    }
}
