using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kogetsu.Library.Core
{
    public class VNRelationshipResetter : MonoBehaviour
    {
        [Serializable]
        public class CharacterResetData
        {
            public VNCharacterSO Character;
            public int DefaultValue = 0;
        }

        [SerializeField] private List<CharacterResetData> _characters = new();

        [Header("Player Name")]
        [SerializeField] private VNPlayerDataSO _playerData;

        public void ResetAll()
        {
            foreach (var data in _characters)
            {
                if (data.Character == null) continue;
                data.Character.SetRelationship(data.DefaultValue);
            }

            _playerData?.ResetName();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:VNCharacterSO");
            foreach (var guid in guids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var so = UnityEditor.AssetDatabase.LoadAssetAtPath<VNCharacterSO>(path);
                if (so == null) continue;
                if (_characters.Exists(d => d.Character == so)) continue;
                _characters.Add(new CharacterResetData { Character = so, DefaultValue = 0 });
            }
        }
#endif
    }
}
