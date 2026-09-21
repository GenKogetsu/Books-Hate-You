using System;

namespace Kogetsu.Library.Core
{
    [Serializable]
    public class VNInteractState
    {
        [MinValue(1)]
        public int TargetAnswerNumber;

        public float ReturnValue;

        public bool ReturnToChoicePhase;

        public bool ChangeRelationship;
        public VNCharacterSO RelationshipCharacter;
        public int RelationshipDelta;

        [SerializeReference] //new: break circular serialization chain (VNConversationNode→VNChoiceNode→VNInteractState)
        public List<VNConversationNode> SubConversation = new();
    }
}
