using UnityEngine;

[CreateAssetMenu(fileName = "Achievement", menuName = "Game/Achievement")]
public sealed class AchievementDefinition : ScriptableObject
{
    [SerializeField] private string _id;
    [SerializeField] private string _title;
    [SerializeField, TextArea(2, 4)] private string _description;
    [SerializeField] private Sprite _icon;
    [SerializeField] private string _iconGlyph = "*";

    public string Id => _id;
    public string Title => _title;
    public string Description => _description;
    public Sprite Icon => _icon;
    public string IconGlyph => _iconGlyph;
}
