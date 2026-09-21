namespace Kogetsu.Library.Core
{
    [RequireComponent(typeof(Animator))]
    public class TitleScreenController : MonoBehaviour
    {
        [SerializeField] protected Animator TitleScreenAnimator;
        [SerializeField] protected List<AnimationClip> AnimationClips;
        [SerializeField] protected float HoldTime = 1.0f;
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (Application.isPlaying) return;

            AssignComponent();
        }
#endif

        protected virtual void Start()
        {
            AssignComponent();
            StartCoroutine(PlayLogoRotine());
        }

        protected virtual void AssignComponent()
        {
            if (!TitleScreenAnimator) this.TryGetComponent(out TitleScreenAnimator);
        }

        protected virtual IEnumerator PlayLogoRotine()
        {
            foreach (var animationClip in AnimationClips)
            {
                TitleScreenAnimator.Play(animationClip.name);

                yield return new WaitForSeconds(animationClip.length + HoldTime);
            }

            yield return null;

            BasicSceneEffectController.Instance.LoadNextScene(1);
        }
    }
}