using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para>Summary :</para>
    /// <para>(TH) : ปุ่มตัวเลือกแต่ละอันในระบบ VN สำหรับแสดงข้อความและรับการกดเลือกจากผู้เล่น</para>
    /// <para>(EN) : Individual choice button in the VN system. Displays answer text and fires a callback when clicked.</para>
    /// </summary>
    public class VNChoiceButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _label;

        private Action<int> _callback;
        private int _answerNumber;

        public void Setup(VNChoiceAnswer answer, Action<int> callback)
        {
            _answerNumber = answer.AnswerNumber;
            _callback = callback;
            if (_label != null) _label.text = answer.AnswerText;
            _button.onClick.RemoveAllListeners();
            _button.onClick.AddListener(OnClick);
        }

        private void OnClick() => _callback?.Invoke(_answerNumber);
    }
}
