
using System;

namespace Kogetsu.Library.Core
{
    [Serializable]
    public class VNDialoguePhaseData : VNChoicePhaseData
    {
        [TextArea(3, 5)]
        public string DialogueText;

        public VNTextSettings TextSettings = new();
    }
}
