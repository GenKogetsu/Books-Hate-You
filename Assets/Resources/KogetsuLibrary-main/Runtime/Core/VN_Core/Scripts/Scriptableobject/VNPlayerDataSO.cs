using UnityEngine;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : เก็บข้อมูลชื่อผู้เล่น — บันทึกผ่าน PlayerPrefs และ sync กับ VNCharacterSO ของผู้เล่น </para>
    /// <para> (EN) : Stores player name — persists via PlayerPrefs and syncs with the player's VNCharacterSO. </para>
    /// </summary>
    [CreateAssetMenu(fileName = "NewVNPlayerData", menuName = "KogetsuLibrary/Core/VN/Player Data")]
    public class VNPlayerDataSO : ScriptableObject
    {
        private const string SAVE_KEY = "VN_PlayerName";

        [Header("Default")]
        [SerializeField] private string _defaultName = "Player";

        [Header("Player Character (optional)")]
        [Tooltip("ถ้ากำหนดไว้ จะ sync CharacterName ของ SO นี้กับชื่อผู้เล่นทุกครั้งที่เปลี่ยน")]
        [SerializeField] private VNCharacterSO _playerCharacter;

        public string PlayerName { get; private set; }

        private void OnEnable() => LoadSavedName();

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// <para> (TH) : ตั้งชื่อผู้เล่น บันทึก PlayerPrefs และ sync กับ VNCharacterSO </para>
        /// <para> (EN) : Sets the player name, saves to PlayerPrefs, and syncs with VNCharacterSO. </para>
        /// </summary>
        public void SetName(string newName)
        {
            PlayerName = newName;
            PlayerPrefs.SetString(SAVE_KEY, newName);
            PlayerPrefs.Save();
            SyncToCharacterSO();
        }

        /// <summary>
        /// <para> (TH) : โหลดชื่อที่บันทึกไว้ (หรือใช้ default ถ้ายังไม่เคย save) </para>
        /// <para> (EN) : Loads the saved name, or uses the default if never saved. </para>
        /// </summary>
        public void LoadSavedName()
        {
            PlayerName = PlayerPrefs.GetString(SAVE_KEY, _defaultName);
            SyncToCharacterSO();
        }

        /// <summary>
        /// <para> (TH) : ลบข้อมูลที่บันทึกไว้และ reset เป็นชื่อ default — เรียกตอน New Game </para>
        /// <para> (EN) : Deletes saved data and resets to the default name. Call on New Game. </para>
        /// </summary>
        public void ResetName()
        {
            PlayerPrefs.DeleteKey(SAVE_KEY);
            PlayerName = _defaultName;
            SyncToCharacterSO();
        }

        // ── Internal ────────────────────────────────────────────────────────────

        private void SyncToCharacterSO()
        {
            if (_playerCharacter != null)
                _playerCharacter.CharacterName = PlayerName;
        }
    }
}
