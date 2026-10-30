using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(AudioSource))]
    [CreateHierarchyMenu("KogetsuLibrary/Core/Controller")]
    public class BasicSceneEffectController : Singleton<BasicSceneEffectController>, ISceneLoader
    {
        [ReadOnly]
        [SerializeField] protected Animator SceneEffectAnimator;
        [SerializeField] protected float AnimatorSpeed = 1f;

        [SerializeField] protected AnimationClip LoadSceneIn;
        [SerializeField] protected AnimationClip LoadSceneOut;

        [Header("Load Scene Audio")]
        [SerializeField] protected AudioSource LoadSceneAudioSource;
        [SerializeField] protected AudioClip LoadSceneAudioClip;

        private bool _isTransitioning;
        public bool IsTransitioning => _isTransitioning;

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!Application.isPlaying)
            {
                AssignComponent();
            }

            SetSceneEffectSpeed(AnimatorSpeed);
        }
#endif
        protected virtual void Start()
        {
            AssignComponent();
            SetSceneEffectSpeed(AnimatorSpeed);
        }

        protected virtual void OnEnable()
        {
            if (EventBus.Instance)
            {
                EventBus.Instance.Subscribe<LoadSceneEvent>(OnLoadSceneEvent);
                EventBus.Instance.Subscribe<OutGameEvent>(OnOutGameEvent);
            }

        }

        protected virtual void OnDisable()
        {
            StopLoadSceneAudio();
            if (_isTransitioning && GameManager.Instance)
                GameManager.Instance.SetGameState(GameState.Normal);
            _isTransitioning = false;

            if (EventBus.Instance)
            {
                EventBus.Instance.Unsubscribe<LoadSceneEvent>(OnLoadSceneEvent);
                EventBus.Instance.Unsubscribe<OutGameEvent>(OnOutGameEvent);
            }
        }

        protected virtual void AssignComponent()
        {
            if (!SceneEffectAnimator) SceneEffectAnimator = this.GetComponent<Animator>();
            if (!LoadSceneAudioSource) LoadSceneAudioSource = this.GetComponent<AudioSource>();
        }

        protected virtual IEnumerator LoadSceneRotine(int sceneBulidIndex)
        {
            if (GameManager.Instance)
                GameManager.Instance.SetGameState(GameState.Pause);

            try
            {
                if (SceneEffectAnimator && LoadSceneIn)
                {
                    SceneEffectAnimator.Play(LoadSceneIn.name);
                    yield return new WaitForSecondsRealtime(GetClipPlaybackDuration(LoadSceneIn));
                }

                SceneManager.LoadScene(sceneBulidIndex);

                if (SceneEffectAnimator && LoadSceneOut)
                {
                    SceneEffectAnimator.Play(LoadSceneOut.name);
                    yield return new WaitForSecondsRealtime(GetClipPlaybackDuration(LoadSceneOut));
                }
            }
            finally
            {
                StopLoadSceneAudio();
                _isTransitioning = false;
                if (GameManager.Instance)
                    GameManager.Instance.SetGameState(GameState.Normal);
            }
        }

        private void OnOutGameEvent(OutGameEvent outGameEvent)
        {
            OutGame();
        }

        public void OutGame()
        {
            if (_isTransitioning || !isActiveAndEnabled) return;
            AssignComponent();
            if (!SceneEffectAnimator || !LoadSceneIn)
            {
                Debug.LogError("Cannot exit: the Fade-in animation state is missing.", this);
                return;
            }
            _isTransitioning = true;
            Time.timeScale = 1f;
            PlayLoadSceneAudio();
            StartCoroutine(OutGameRoutine());
        }

        private IEnumerator OutGameRoutine()
        {

            if (GameManager.Instance)
                GameManager.Instance.SetGameState(GameState.Pause);
            try
            {
                if (SceneEffectAnimator && LoadSceneIn)
                {
                    SceneEffectAnimator.Play(LoadSceneIn.name);
                    yield return new WaitForSecondsRealtime(GetClipPlaybackDuration(LoadSceneIn));
                }
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
            finally
            {

                StopLoadSceneAudio();
                _isTransitioning = false;
                if (GameManager.Instance)
                    GameManager.Instance.SetGameState(GameState.Normal);
            }
        }

        private float GetClipPlaybackDuration(AnimationClip clip)
        {
            if (!clip)
                return 0f;

            float speed = SceneEffectAnimator ? Mathf.Abs(SceneEffectAnimator.speed) : Mathf.Abs(AnimatorSpeed);
            return clip.length / Mathf.Max(speed, 0.01f);
        }

        private void OnLoadSceneEvent(LoadSceneEvent loadSceneEvent)
        {
            if (_isTransitioning) return;
            if (loadSceneEvent.SceneName == "Reload")
            {
                ReloadScene();
                Debug.Log($"LoadSceneEvent: Reload scene: {SceneManager.GetActiveScene().name}");
            }

            else if (!string.IsNullOrEmpty(loadSceneEvent.SceneName))
            {
                LoadScene(loadSceneEvent.SceneName);
                Debug.Log($"LoadSceneEvent: Load scene by name: {loadSceneEvent.SceneName}");
            }

            else if (loadSceneEvent.SceneIndex >= 0 && loadSceneEvent.SceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                LoadScene(loadSceneEvent.SceneIndex);
                Debug.Log($"LoadSceneEvent: Load scene by index: {loadSceneEvent.SceneIndex}");
            }

        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : ค้นหาชื่อ Scene ใน Build Settings แล้วส่ง build index ไปยัง LoadScene(int) </para>
        /// <para> (EN) : Finds the scene name in Build Settings and delegates its build index to LoadScene(int). </para>
        /// </summary>
        #endregion
        private static int FindBuildIndex(string sceneName)
        {
            for (int index = 0; index < SceneManager.sceneCountInBuildSettings; index++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(index);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != sceneName)
                    continue;
                return index;
            }
            return -1;
        }

        public void LoadScene(string sceneName)
        {
            int index = FindBuildIndex(sceneName);
            if (index >= 0) LoadScene(index);
            else Debug.LogError($"Scene '{sceneName}' is not in Build Settings.", this);
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : คืน Time.timeScale เป็น 1 แล้วเริ่ม transition หากยังไม่มีการโหลด Scene อยู่ </para>
        /// <para> (EN) : Restores Time.timeScale to 1 and starts a transition unless one is already running. </para>
        /// </summary>
        #endregion
        public void LoadScene(int buildIndex)
        {
            TryBeginTransition(buildIndex, null, null);
        }

        public bool TryLoadSceneWithEvent(int buildIndex, Action onAccepted = null)
        {
            return TryBeginTransition(buildIndex,
                new LoadSceneEvent(SceneIndex: buildIndex), onAccepted);
        }

        public bool TryLoadSceneWithEvent(string sceneName, Action onAccepted = null)
        {
            int index = FindBuildIndex(sceneName);
            if (index < 0)
            {
                Debug.LogError($"Scene '{sceneName}' is not in Build Settings.", this);
                return false;
            }
            return TryBeginTransition(index,
                new LoadSceneEvent(SceneName: sceneName), onAccepted);
        }

        public bool TryReloadSceneWithEvent(Action onAccepted = null)
        {
            return TryBeginTransition(SceneManager.GetActiveScene().buildIndex,
                new LoadSceneEvent(SceneName: "Reload"), onAccepted);
        }

        private bool TryBeginTransition(int buildIndex, LoadSceneEvent? notification, Action onAccepted)
        {
            if (_isTransitioning || !isActiveAndEnabled ||
                buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
                return false;

            _isTransitioning = true;
            try
            {
                AssignComponent();
                Time.timeScale = 1f;
                onAccepted?.Invoke();
                if (notification.HasValue && EventBus.Instance)
                    EventBus.Instance.Publish(notification.Value);
                PlayLoadSceneAudio();
                StartCoroutine(LoadSceneRotine(buildIndex));
                return true;
            }
            catch
            {
                _isTransitioning = false;
                StopLoadSceneAudio();
                throw;
            }
        }

        protected virtual void PlayLoadSceneAudio()
        {
            if (!LoadSceneAudioSource || !LoadSceneAudioClip)
                return;

            LoadSceneAudioSource.clip = LoadSceneAudioClip;
            LoadSceneAudioSource.Play();
        }

        protected virtual void StopLoadSceneAudio()
        {
            if (LoadSceneAudioSource)
                LoadSceneAudioSource.Stop();
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : รอ Fade-in จบ คืนสถานะเกม แล้วโหลด active Scene ใหม่ </para>
        /// <para> (EN) : Reloads the active scene after Fade-in completes and the game state is restored. </para>
        /// </summary>
        #endregion
        public void ReloadScene()
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : บวก offset เข้ากับ index ของ active Scene แล้วส่งต่อ LoadScene(int); ผู้เรียกต้องระวัง index ที่เกินรายการ Build Settings </para>
        /// <para> (EN) : Adds an offset to the active scene index and delegates to LoadScene(int); callers must avoid indices outside Build Settings. </para>
        /// </summary>
        #endregion
        public void LoadNextScene(int next)
        {
            LoadScene(SceneManager.GetActiveScene().buildIndex + next);
        }

        public void SetSceneEffectSpeed(float speed)
        {
            SceneEffectAnimator.speed = speed;
        }
    }
}
