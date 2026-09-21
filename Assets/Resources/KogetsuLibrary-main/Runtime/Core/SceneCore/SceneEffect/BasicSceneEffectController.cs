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
                EventBus.Instance.Subscribe<EventName>(OnEventName);
            }

        }

        protected virtual void OnDisable()
        {
            if (EventBus.Instance)
            {
                EventBus.Instance.Unsubscribe<EventName>(OnEventName);
            }
        }

        protected virtual void AssignComponent()
        {
            if (!SceneEffectAnimator) SceneEffectAnimator = this.GetComponent<Animator>();
        }

        protected virtual void OnEventName(EventName eventName)
        {
            Action action = eventName.Name switch
            {
                "LoadNextScene" => () => LoadNextScene(1),
                _ => () => Debug.LogWarning("Unknown Event")
            };

            action?.Invoke();
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