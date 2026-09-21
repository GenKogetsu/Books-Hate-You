using UnityEngine.EventSystems;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    [RequireComponent(typeof(Animator))]
    public class BasicButtonStateController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [ReadOnly]
        [SerializeField] protected Animator ButtonAnimator;

        [Header("Button State Animation Name")]
        [SerializeField] protected string IsHoverParameter;
        [SerializeField] protected string IsReleaseParameter;
            
        [Header("PublicEventsOnClick")]
        [SerializeField] protected List<string> PublicEventsOnHover;
        [SerializeField] protected List<string> PublicEventsOnExit;
        [SerializeField] protected List<string> PublicEventsOnClick;

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) => OnPointerEnter(eventData);
        void IPointerExitHandler.OnPointerExit(PointerEventData eventData) => OnPointerExit(eventData);
        void IPointerDownHandler.OnPointerDown(PointerEventData eventData) => OnPointerDown(eventData);

        protected virtual void OnValidate()
        {
            if (!ButtonAnimator) this.TryGetComponent<Animator>(out ButtonAnimator);
        }

        protected virtual void OnPointerEnter(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(IsHoverParameter))
                ButtonAnimator.SetBool(IsHoverParameter, true);

            if (PublicEventsOnHover.Count != 0)
            {
                if (!EventBus.Instance) return;

                foreach (string eventName in PublicEventsOnHover)
                {
                    if (string.IsNullOrEmpty(eventName)) continue;

                    EventBus.Instance.Publish(new EventName(eventName));
                }
            }
        }

        protected virtual void OnPointerExit(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(IsHoverParameter))
                ButtonAnimator.SetBool(IsHoverParameter, false);

            if (PublicEventsOnExit.Count != 0)
            {
                if (!EventBus.Instance) return;

                foreach (string eventName in PublicEventsOnExit)
                {
                    if (string.IsNullOrEmpty(eventName)) continue;

                    EventBus.Instance.Publish(new EventName(eventName));
                }
            }
        }

        protected virtual void OnPointerDown(PointerEventData eventData)
        {
            if (!string.IsNullOrEmpty(IsReleaseParameter))
                ButtonAnimator.SetBool(IsReleaseParameter, true);

            if (PublicEventsOnClick.Count != 0)
            {
                if (!EventBus.Instance) return;

                foreach (string eventName in PublicEventsOnClick)
                {
                    if (string.IsNullOrEmpty(eventName)) continue;

                    EventBus.Instance.Publish(new EventName(eventName));
                }
            }

        }
    }
}