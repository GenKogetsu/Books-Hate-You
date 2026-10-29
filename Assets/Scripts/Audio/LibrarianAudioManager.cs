#region Imports
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
#endregion
#region LibrarianAudioManager

public sealed class LibrarianAudioManager : BaseAudioManager
{
    #region Audio sources and chase state
    [Header("Run SFX")]
    [SerializeField] private AudioSource _runSfxSource;

    [Header("Chase BGM")]
    [SerializeField, FormerlySerializedAs("_chaseSource")] private AudioSource _chaseBgmSource;

    [Header("Level BGM")]
    [SerializeField] private AudioMixerGroup _levelBgmGroup;

    private static readonly HashSet<LibrarianAudioManager> ActiveChases = new();
    private static readonly Dictionary<AudioSource, bool> OriginalMuteStates = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetChaseState()
    {
        ActiveChases.Clear();
        OriginalMuteStates.Clear();
    }

    #endregion
    #region Unity lifecycle
    private void Awake()
    {
        if (!_runSfxSource || !_chaseBgmSource)
            Debug.LogWarning("Assign separate Run SFX and Chase BGM AudioSources.", this);
    }

    private void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StopAll();
    }

    #endregion
    #region Event handlers
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ActiveChases.Contains(this))
            MuteLevelBgm();
    }

    #endregion
    #region Chase playback and mixer control
    public void PlayRunSfx() => PlayLoop(_runSfxSource);
    public void StopRunSfx() => Stop(_runSfxSource);

    public void PlayChaseBgm()
    {
        if (!_chaseBgmSource || !_chaseBgmSource.clip)
            return;

        ActiveChases.Add(this);
        MuteLevelBgm();
        PlayLoop(_chaseBgmSource);
    }

    public void StopChaseBgm()
    {
        Stop(_chaseBgmSource);
        ActiveChases.Remove(this);
        if (ActiveChases.Count == 0)
            RestoreLevelBgm();
    }

    private void MuteLevelBgm()
    {
        if (!_levelBgmGroup)
        {
            Debug.LogWarning("Assign the Level BGM mixer group to mute it during chase.", this);
            return;
        }

        foreach (AudioSource source in FindObjectsByType<AudioSource>(FindObjectsInactive.Include))
        {
            if (!source || source.outputAudioMixerGroup != _levelBgmGroup)
                continue;

            if (!OriginalMuteStates.ContainsKey(source))
                OriginalMuteStates.Add(source, source.mute);

            source.mute = true;
        }
    }

    private static void RestoreLevelBgm()
    {
        foreach (KeyValuePair<AudioSource, bool> entry in OriginalMuteStates)
        {
            if (entry.Key)
                entry.Key.mute = entry.Value;
        }

        OriginalMuteStates.Clear();
    }

    public void StopAll()
    {
        StopRunSfx();
        StopChaseBgm();
    }
    #endregion
}
#endregion
