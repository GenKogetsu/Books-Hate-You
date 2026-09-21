using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    /// <summary>
    /// <para>Summary :</para>
    /// <para>(TH) : ตัวช่วย advance บทสนทนา VN — เพิ่มลงใน GameObject ใดก็ได้ในฉาก
    ///              หรือเชื่อมกับ Button.onClick เพื่อให้ผู้เล่นกดข้ามได้
    ///              เป็นทางเลือกแบบเบาสำหรับกรณีที่ยังไม่ได้ตั้งค่า PlayerInput</para>
    /// <para>(EN) : Lightweight VN dialogue advancer. Attach to any GameObject or wire to
    ///              a Button.onClick to let the player advance. Alternative to full PlayerInput setup.</para>
    /// </summary>
    public class VNDialogueAdvancer : MonoBehaviour
    {
        [Header("Observer Channel")]
        [Required]
        [SerializeField] private BasicMovementInputObserverSO _observerChannel;

        [Header("Options")]
        [Tooltip("กดปุ่มใดก็ได้บน keyboard เพื่อ advance (เพิ่มเติมจากการกด Advance())")]
        [SerializeField] private bool _anyKeyAdvances = true;

        private void Update()
        {
            if (_anyKeyAdvances && Input.anyKeyDown)
                Advance();
        }

        /// <summary>
        /// <para>(TH) : เรียกเมธอดนี้เพื่อ advance บทสนทนา — ใช้กับ Button.onClick ได้โดยตรง</para>
        /// <para>(EN) : Call this to advance the dialogue. Can be wired directly to Button.onClick.</para>
        /// </summary>
        public void Advance()
        {
            if (_observerChannel == null) return;
            _observerChannel.SendInteractionSignal();
        }
    }
}
