using System;
using UnityEngine.SceneManagement;
using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    [RequireComponent(typeof(Animator))]
    [CreateHierarchyMenu("KogetsuLibrary/Core/Controller")]
    public class BasicSceneEffectController : Singleton<BasicSceneEffectController>, ISceneLoader
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
        /// <para> (TH) : หา build index ด้วย SceneManager.GetSceneByName แล้วส่งไปยัง LoadScene(int); API นี้ไม่ได้ค้นหา Scene ที่ยังไม่โหลดจาก Build Settings ด้วยชื่อให้เอง </para>
        /// <para> (EN) : Resolves the build index with SceneManager.GetSceneByName and delegates to LoadScene(int); it does not search unloaded Build Settings scenes by name. </para>
        /// </summary>
        #endregion
        public void LoadScene(string sceneName)
        {
            int sceneBulidIndex = SceneManager.GetSceneByName(sceneName).buildIndex;
            LoadScene(sceneBulidIndex);
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : คืน Time.timeScale เป็น 1 แล้วเริ่ม LoadSceneRotine ด้วย index ที่รับมา; แต่ละการเรียกสร้าง coroutine ของตัวเองและไม่มีการกัน transition ซ้อน </para>
        /// <para> (EN) : Restores Time.timeScale to 1 and starts LoadSceneRotine with the supplied index; each request starts its own coroutine without transition deduplication. </para>
        /// </summary>
        #endregion
        public void LoadScene(int buildIndex)
        {
            Time.timeScale = 1f;
            StartCoroutine(LoadSceneRotine(buildIndex));
        }

        #region Documentation - XML
        /// <summary>
        /// <para> Summary : </para>
        /// <para> (TH) : อ่าน index ของ active Scene แล้วเรียก LoadScene(int) เพื่อใช้ขั้นตอน transition จุดเดียวกัน </para>
        /// <para> (EN) : Reads the active scene index and delegates to LoadScene(int) to share the transition path. </para>
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
