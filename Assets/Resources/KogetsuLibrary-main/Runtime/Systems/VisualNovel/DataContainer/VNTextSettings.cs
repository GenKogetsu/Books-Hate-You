using System;
using TMPro;

namespace Kogetsu.Library.Core
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : โครงสร้างข้อมูลสำหรับตั้งค่ารูปแบบข้อความในแต่ละเฟส </para>
    /// <para> (EN) : Data structure for configuring text formatting in each phase. </para>
    /// </summary>
    #endregion
    [Serializable]
    public class VNTextSettings
    {
        public bool OverrideGlobalSettings = false;
        public TMP_FontAsset CustomFont;
        public float FontSize = 36f;
        public Color FontColor = Color.white;
        public TextAlignmentOptions Alignment = TextAlignmentOptions.TopLeft;
        public float TypingSpeed = 30f;
    }
}
