namespace Kogetsu.Library.Core
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : สัญญาคำสั่งเปลี่ยน Scene โดยให้ implementation รับผิดชอบ transition และการรับ event </para>
    /// <para> (EN) : Scene navigation API; effects and event handling belong to its implementation. </para>
    /// </summary>
    #endregion
    public interface ISceneLoader
    {
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ร้องขอฉากตามชื่อ โดย implementation รับผิดชอบการ resolve ชื่อและ transition </para>
        /// <para> (EN) : Requests a scene by name; name resolution and transitions belong to the implementation. </para>
        /// </summary>
        #endregion
        void LoadScene(string sceneName);
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ร้องขอฉากด้วย build index ผ่าน implementation ของ scene loader </para>
        /// <para> (EN) : Requests a scene by build index through the scene loader implementation. </para>
        /// </summary>
        #endregion
        void LoadScene(int buildIndex);
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ร้องขอโหลด Scene ที่ active อยู่ใหม่ </para>
        /// <para> (EN) : Requests reloading the currently active scene. </para>
        /// </summary>
        #endregion
        void ReloadScene();
        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ร้องขอ Scene ที่มี build index เท่ากับ Scene ปัจจุบันบวก next; next เป็นค่าลบได้ </para>
        /// <para> (EN) : Requests the active scene's build index plus next; negative offsets are permitted. </para>
        /// </summary>
        #endregion
        void LoadNextScene(int next);
    }
}
