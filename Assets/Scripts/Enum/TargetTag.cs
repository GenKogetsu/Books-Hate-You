[System.Flags]
public enum TargetTag
{
    None = 0,
    Untagged = 1 << 0,
    Respawn = 1 << 1,
    Finish = 1 << 2,
    EditorOnly = 1 << 3,
    MainCamera = 1 << 4,
    Player = 1 << 5,
    GameController = 1 << 6,
    Librarian = 1 << 7,
    SFX_Channel_KogetsuLibrary = 1 << 8,
    BMG_Channel_KogetsuLibrary = 1 << 9,
    Voiceover_Channel_KogetsuLibrary = 1 << 10,
}

public static class TargetTagExtensions
{
    public static bool MatchesUnityTag(this TargetTag targetTags, string unityTag)
    {
        return
            ((targetTags & TargetTag.Untagged) != 0 && unityTag == "Untagged") ||
            ((targetTags & TargetTag.Respawn) != 0 && unityTag == "Respawn") ||
            ((targetTags & TargetTag.Finish) != 0 && unityTag == "Finish") ||
            ((targetTags & TargetTag.EditorOnly) != 0 && unityTag == "EditorOnly") ||
            ((targetTags & TargetTag.MainCamera) != 0 && unityTag == "MainCamera") ||
            ((targetTags & TargetTag.Player) != 0 && unityTag == "Player") ||
            ((targetTags & TargetTag.GameController) != 0 && unityTag == "GameController") ||
            ((targetTags & TargetTag.Librarian) != 0 && unityTag == "Librarian") ||
            ((targetTags & TargetTag.SFX_Channel_KogetsuLibrary) != 0 && unityTag == "SFX_Channel.KogetsuLibrary") ||
            ((targetTags & TargetTag.BMG_Channel_KogetsuLibrary) != 0 && unityTag == "BMG_Channel.KogetsuLibrary") ||
            ((targetTags & TargetTag.Voiceover_Channel_KogetsuLibrary) != 0 && unityTag == "Voiceover_Channel.KogetsuLibrary");
    }
}
