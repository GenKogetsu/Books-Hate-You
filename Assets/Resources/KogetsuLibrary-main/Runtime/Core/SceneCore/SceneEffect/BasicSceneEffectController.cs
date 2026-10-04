using System;
using UnityEngine.SceneManagement;
using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    [RequireComponent(typeof(Animator))]
    [CreateHierarchyMenu("KogetsuLibrary/Core/Controller")]
    public class BasicSceneEffectController : Singleton<BasicSceneEffectController>
    {
        [ReadOnly]
        [SerializeField] protected Animator SceneEffectAnimator;
        [SerializeField] protected float AnimatorSpeed = 1f;

        [SerializeField] protected AnimationClip LoadSceneIn;
        [SerializeField] protected AnimationClip LoadSceneOut;

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
            if (EventBus.Instance)
            {
                EventBus.Instance.Unsubscribe<LoadSceneEvent>(OnLoadSceneEvent);
            }
        }

        protected virtual void AssignComponent()
        {
            if (!SceneEffectAnimator) SceneEffectAnimator = this.GetComponent<Animator>();
        }

        protected virtual IEnumerator LoadSceneRotine(int sceneBulidIndex)
        {
            GameManager.Instance.SetGameState(GameState.Pause);

            if (LoadSceneIn)
            {
                SceneEffectAnimator.Play(LoadSceneIn.name);
                yield return new WaitForSeconds(LoadSceneIn.length * AnimatorSpeed);
            }

            SceneManager.LoadScene(sceneBulidIndex);

            if (LoadSceneOut)
            {
                yield return new WaitForSeconds(LoadSceneOut.length * AnimatorSpeed);
            }

            GameManager.Instance.SetGameState(GameState.Normal);
        }

        private void OnLoadSceneEvent(LoadSceneEvent loadSceneEvent)
        {
            if (loadSceneEvent.SceneIndex < 0 || loadSceneEvent.SceneName == "Reload")
            {
                ReloadScene();
                Debug.LogWarning($"LoadSceneEvent: Invalid scene index: {loadSceneEvent.SceneIndex}. Loading current scene instead.");
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

        public void LoadScene(string sceneName)
        {
            Time.timeScale = 1f;
            int sceneBulidIndex = SceneManager.GetSceneByName(sceneName).buildIndex;
            StartCoroutine(LoadSceneRotine(sceneBulidIndex));
        }

        public void LoadScene(int buildIndex)
        {
            Time.timeScale = 1f;
            StartCoroutine(LoadSceneRotine(buildIndex));
        }

        public void ReloadScene()
        {
            Time.timeScale = 1f;
            int sceneBulidIndex = SceneManager.GetActiveScene().buildIndex;
            StartCoroutine(LoadSceneRotine(sceneBulidIndex));
        }

        public void LoadNextScene(int next)
        {
            Time.timeScale = 1f;
            int sceneBulidIndex = SceneManager.GetActiveScene().buildIndex + next;
            StartCoroutine(LoadSceneRotine(sceneBulidIndex));
        }

        public void SetSceneEffectSpeed(float speed)
        {
            SceneEffectAnimator.speed = speed;
        }
    }
}