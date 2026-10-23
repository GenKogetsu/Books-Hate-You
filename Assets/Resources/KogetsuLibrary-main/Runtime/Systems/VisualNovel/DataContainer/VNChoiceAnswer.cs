using System;

namespace Kogetsu.Library.Core
{
    [Serializable]
    public struct VNChoiceAnswer
    {
        [TextArea(3, 5)]
        public string AnswerText;

        [Min(1)]
        public int AnswerNumber;
    }
}