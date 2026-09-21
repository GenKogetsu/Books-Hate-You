using System;

namespace Kogetsu.Library.Core
{
    [Serializable]
    public class VNChoiceNode
    {
        public VNQuestionState QuestionState;

        public VNAnswerState AnswerState;

        public List<VNInteractState> InteractStates;
    }
}