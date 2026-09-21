#if UNITY_EDITOR

namespace Kogetsu.Library.Assistant
{
    [CreateAssetMenu(fileName = "TagSettings", menuName = "KogetsuLibrary/Assistant/Tag Settings")]
    public class TagSettingsSO : ScriptableObject
    {
        [Header("Settings")]
        public List<string> RequiredTags = new()
        {
           "SFX_Channel",
            "BMG_Channel",
            "Voiceover_Channel"
        };
    }
}
#endif