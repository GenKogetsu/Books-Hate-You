namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : ไฟล์ข้อมูลบทสนทนาที่เก็บรวบรวมโหนดเนื้อเรื่องทั้งหมดในบทนั้น </para>
    /// <para> (EN) : Conversation data file storing all story nodes for the chapter. </para>
    /// </summary>
    [CreateAssetMenu(fileName = "Scene_0", menuName = "KogetsuLibrary/Core/VN/Scene")]
    public class VNSceneSO : ScriptableObject
    {
        public string SceneName;
        public ushort Index;

        [Header("End Transition")]
        public bool UseEndScene;
        public int  EndSceneIndex;

        public List<VNConversationNode> Conversations = new();
    }
}