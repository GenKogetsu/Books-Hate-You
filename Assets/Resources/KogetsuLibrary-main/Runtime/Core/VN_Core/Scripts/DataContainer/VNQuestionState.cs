using System;

namespace Kogetsu.Library.Core
{
    [Serializable]
    public class VNQuestionState
    {
        public bool UseEnterPhase;
        public VNDialoguePhaseData EnterPhase;

        public VNDialoguePhaseData MainPhase;
    }
}