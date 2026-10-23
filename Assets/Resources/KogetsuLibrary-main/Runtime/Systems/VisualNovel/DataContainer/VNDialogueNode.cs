using System;

namespace Kogetsu.Library.Core
{
    #region Documentation - XML
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : โหนดบทสนทนาหลักที่รวบรวมเฟสการทำงานต่างๆ เข้าด้วยกัน </para>
    /// <para> (EN) : Main conversation node aggregating different execution phases. </para>
    /// </summary>
    #endregion
    [Serializable]
    public class VNDialogueNode
    {
        public bool UseEnterPhase;
        public VNDialoguePhaseData EnterPhase;

        public VNDialoguePhaseData MainPhase;

        public bool UseExitPhase;
        public VNDialoguePhaseData ExitPhase;
    }
}