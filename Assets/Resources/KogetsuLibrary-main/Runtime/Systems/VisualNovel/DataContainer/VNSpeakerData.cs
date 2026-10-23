using System;

namespace Kogetsu.Library.Core
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ข้อมูลของตัวละครและรายการคำสั่งทั้งหมดในเฟสนั้น </para>
    /// <para> (EN) : Character data and their list of action commands for the phase. </para>
    /// </summary>
    #endregion
    [Serializable]
    public class VNSpeakerData
    {
        [Required]
        public VNCharacterSO Character;

        public VNNameDisplayMode NameDisplayMode = VNNameDisplayMode.Text;
        public string CustomName;

        public List<VNAction> Actions = new();
    }
}