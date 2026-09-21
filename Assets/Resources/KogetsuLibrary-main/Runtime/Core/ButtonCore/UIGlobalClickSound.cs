using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : วางบน Canvas หรือ GameObject ที่ persistent — ทุกครั้งที่คลิก Selectable ใดก็ตามในฉาก จะเล่นเสียง SFX อัตโนมัติ </para>
    /// <para> (EN) : Place on a Canvas or persistent GameObject. Automatically plays SFX whenever any interactable Selectable is clicked. </para>
    /// </summary>
    public class UIGlobalClickSound : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private AudioObserverSO _sfxObserver;
        [SerializeField] private AudioClip        _clickClip;

        private readonly List<RaycastResult> _raycastResults = new();

        private void Update()
        {
            if (!Input.GetMouseButtonDown(0)) return;
            if (EventSystem.current == null)   return;

            _raycastResults.Clear();

            var pointer = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            EventSystem.current.RaycastAll(pointer, _raycastResults);

            foreach (var result in _raycastResults)
            {
                // เล่นเสียงเมื่อโดน Selectable ที่ interactable = true ตัวแรก
                if (result.gameObject.GetComponentInParent<Selectable>() is { interactable: true })
                {
                    _sfxObserver?.SendSfxSignal(_clickClip);
                    return;
                }
            }
        }
    }
}
