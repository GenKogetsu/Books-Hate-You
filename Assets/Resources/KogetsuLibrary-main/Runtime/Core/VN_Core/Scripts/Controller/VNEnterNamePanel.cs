using System;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using NaughtyAttributes;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para> Summary : </para>
    /// <para> (TH) : Panel กลางหน้าจอสำหรับกรอกชื่อผู้เล่น — เปิด/ปิดผ่าน Animator, รองรับเสียงพิมพ์และเสียงคลิก </para>
    /// <para> (EN) : Center-screen panel for player name input. Uses Animator On/Off states, supports typing and click SFX. </para>
    /// </summary>
    public class VNEnterNamePanel : MonoBehaviour
    {
        [Header("UI References")]
        [Required]
        [SerializeField] private TMP_InputField _inputField;

        [Required]
        [SerializeField] private Button _confirmButton;

        [Required]
        [SerializeField] private Animator _animator;

        [Header("Animation State Names")]
        [SerializeField] private string _showStateName = "On";
        [SerializeField] private string _hideStateName = "Off";

        [Header("Audio")]
        [SerializeField] private AudioObserverSO _sfxObserver;
        [SerializeField] private AudioClip        _typingClip;
        [SerializeField] private AudioClip        _confirmClickClip;

        [Header("Data")]
        [Required]
        [SerializeField] private VNPlayerDataSO _playerData;

        private Action _onConfirmed;

        private void Start()
        {
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _inputField.onValueChanged.AddListener(OnInputValueChanged);
        }

        private void OnDestroy()
        {
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            _inputField.onValueChanged.RemoveListener(OnInputValueChanged);
        }

        // ── Public API ──────────────────────────────────────────────────────────

        /// <summary>
        /// <para> (TH) : เล่น animation เปิด panel — callback จะถูกเรียกหลัง animation ปิดเสร็จ </para>
        /// <para> (EN) : Plays the show animation. Callback fires after the hide animation finishes. </para>
        /// </summary>
        public void Show(Action onConfirmed)
        {
            _onConfirmed = onConfirmed;
            _animator.Play(_showStateName);
            _inputField.ActivateInputField();
            _inputField.Select();
        }

        /// <summary>
        /// <para> (TH) : เล่น animation ปิด panel (ไม่รอ callback) </para>
        /// <para> (EN) : Plays the hide animation without waiting for a callback. </para>
        /// </summary>
        public void Hide() => _animator.Play(_hideStateName);

        // ── Internal ────────────────────────────────────────────────────────────

        private void OnInputValueChanged(string _)
        {
            if (_typingClip != null)
                _sfxObserver?.SendSfxSignal(_typingClip);
        }

        private void OnConfirmClicked()
        {
            var name = _inputField.text.Trim();
            if (string.IsNullOrEmpty(name)) return; // ไม่ยอมให้ชื่อว่าง

            _playerData.SetName(name);

            if (_sfxObserver != null && _confirmClickClip != null)
                _sfxObserver.SendSfxSignal(_confirmClickClip);

            var callback = _onConfirmed;
            _onConfirmed = null;
            StartCoroutine(HideRoutine(callback));
        }

        /// <summary>
        /// <para> (TH) : เล่น animation ปิด รอจนจบ แล้วค่อย invoke callback เพื่อให้ VNSceneReader ดำเนินต่อ </para>
        /// <para> (EN) : Plays hide animation, waits for it to finish, then invokes callback to unblock VNSceneReader. </para>
        /// </summary>
        private IEnumerator HideRoutine(Action callback)
        {
            _animator.Play(_hideStateName);
            yield return null; // รอ 1 frame ให้ Animator เริ่ม state ใหม่ก่อน
            var length = _animator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(length);
            callback?.Invoke();
        }
    }
}
