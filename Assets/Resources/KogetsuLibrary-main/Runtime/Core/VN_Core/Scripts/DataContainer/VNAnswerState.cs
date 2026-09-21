using System;

namespace Kogetsu.Library.Core
{
    [Serializable]
    public class VNAnswerState
    {
        public bool UseEnterPhase;
        public VNDialoguePhaseData EnterPhase;

        public VNChoicePhaseData MainPhase;

        public bool UseExitPhase;
        public VNChoicePhaseData ExitPhase;

        public List<VNChoiceAnswer> Choices;
    }
}