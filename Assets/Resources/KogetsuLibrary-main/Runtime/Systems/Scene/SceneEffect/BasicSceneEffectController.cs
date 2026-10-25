using System;
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
        private bool _isReloading;

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
            }

        }

        protected virtual void OnDisable()
        {
            StopLoadSceneAudio();
            _isTransitioning = false;
            _isReloading = false;

            if (EventBus.Instance)
            {
                EventBus.Instance.Unsubscribe<LoadSceneEvent>(OnLoadSceneEvent);
            }
        }

        protected virtual void AssignComponent()
        {
            if (!SceneEffectAnimator) SceneEffectAnimator = this.GetComponent<Animator>();
            if (!LoadSceneAudioSource) LoadSceneAudioSource = this.GetComponent<AudioSource>();
        }

        protected virtual IEnumerator LoadSceneRotine(int sceneBulidIndex)
        {
            GameManager.Instance.SetGameState(GameState.Pause);

            if (LoadSceneIn)
            {
                SceneEffectAnimator.Play(LoadSceneIn.name);
                yield return WaitForAnimationToFinish(LoadSceneIn);
            }

            if (_isReloading)
            {
                GameManager.Instance.SetGameState(GameState.Normal);
                _isReloading = false;
            }

            SceneManager.LoadScene(sceneBulidIndex);

            if (LoadSceneOut)
            {
                SceneEffectAnimator.Play(LoadSceneOut.name);
                yield return WaitForAnimationToFinish(LoadSceneOut);
            }

            StopLoadSceneAudio();
            _isTransitioning = false;
            GameManager.Instance.SetGameState(GameState.Normal);
        }

        private System.Collections.IEnumerator WaitForAnimationToFinish(AnimationClip clip)
        {
            if (!SceneEffectAnimator || !clip)
                yield break;

            int stateHash = Animator.StringToHash(clip.name);
            bool hasStarted = false;
            yield return null;

            while (SceneEffectAnimator)
            {
                AnimatorStateInfo state = SceneEffectAnimator.GetCurrentAnimatorStateInfo(0);
                if (state.shortNameHash == stateHash)
                {
                    hasStarted = true;
                    if (state.normalizedTime >= 1f)
                        break;
                }
                else if (hasStarted)
                {
                    // The Animator may transition away on the frame the clip finishes.
                    break;
                }

                yield return null;
            }
        }

        private void OnLoadSceneEvent(LoadSceneEvent loadSceneEvent)
        {
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
        public void LoadScene(string sceneName)
        {
            for (int index = 0; index < SceneManager.sceneCountInBuildSettings; index++)
            {
                string path = SceneUtility.GetScenePathByBuildIndex(index);
                if (System.IO.Path.GetFileNameWithoutExtension(path) != sceneName)
                    continue;

                LoadScene(index);
                return;
            }

            Debug.LogError($"Scene '{sceneName}' is not in Build Settings.", this);
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
            if (_isTransitioning)
                return;

            _isTransitioning = true;
            AssignComponent();
            Time.timeScale = 1f;
            PlayLoadSceneAudio();
            StartCoroutine(LoadSceneRotine(buildIndex));
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
            if (_isTransitioning)
                return;

            _isReloading = true;
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
