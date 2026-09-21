using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para>Summary :</para>
    /// <para>(TH) : แผงควบคุมปุ่มตัวเลือก VN ทำหน้าที่สร้าง/ลบปุ่มตัวเลือกและส่งคำตอบที่เลือกกลับไปยัง VNSceneReader</para>
    /// <para>(EN) : VN choice button panel. Spawns and removes choice buttons, then forwards the selected answer to VNSceneReader.</para>
    /// </summary>
    public class VNChoicePanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _buttonContainer;
        [SerializeField] private VNChoiceButton _buttonPrefab;

        [Header("Animation")]
        [SerializeField] private Animator _panelAnimator;
        [SerializeField] private string   _showStateName = "On";
        [SerializeField] private string   _hideStateName = "Off";

        private readonly List<VNChoiceButton> _activeButtons = new();
        private Action<int> _onSelected;

        private void Start() { }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// <para>(TH) : เล่น animation เปิดแผงและสร้างปุ่มตัวเลือกจากรายการ choices</para>
        /// <para>(EN) : Plays show animation and instantiates a button for each choice.</para>
        /// </summary>
        public void ShowChoices(List<VNChoiceAnswer> choices, Action<int> onSelected)
        {
            _onSelected = onSelected;
            ClearButtons();

            _panelAnimator?.Play(_showStateName);

            foreach (var choice in choices)
            {
                var btn = Instantiate(_buttonPrefab, _buttonContainer);
                btn.Setup(choice, OnButtonClicked);
                _activeButtons.Add(btn);
            }
        }

        /// <summary>
        /// <para>(TH) : เล่น animation ปิด รอจนจบ แล้วค่อยล้างปุ่ม — yield return จาก VNSceneReader เพื่อรอให้เสร็จก่อนดำเนินต่อ</para>
        /// <para>(EN) : Plays hide animation, waits for it to finish, then clears buttons. Yield return this from VNSceneReader.</para>
        /// </summary>
        public IEnumerator HideChoices()
        {
            if (_panelAnimator != null)
            {
                _panelAnimator.Play(_hideStateName);
                yield return null; // รอ 1 frame ให้ Animator เริ่ม state ใหม่ก่อน
                var length = _panelAnimator.GetCurrentAnimatorStateInfo(0).length;
                yield return new WaitForSeconds(length);
            }

            ClearButtons();
        }

        // ── Internal ────────────────────────────────────────────────────────────

        private void OnButtonClicked(int answerNumber) => _onSelected?.Invoke(answerNumber);

        private void ClearButtons()
        {
            foreach (var btn in _activeButtons)
                if (btn != null) Destroy(btn.gameObject);
            _activeButtons.Clear();
        }
    }
}
