using System;
using Kogetsu.Library.Core;

[Serializable]
public class VNConversationNode
{
    public VNConversationMode ConversationMode;

    public VNDialogueNode DialogueNode;
    public VNChoiceNode ChoiceNode;
}