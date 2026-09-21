using TMPro;
using UnityEngine.InputSystem;

namespace Kogetsu.Library.Core
{
    public abstract class BaseSkillMono : MonoBehaviour, ISkill
    {
        [Header("Skill Settings")]
        [Required]
        [SerializeField] protected BasicSkillDataSO SkillData;

        [ReadOnly , MinValue(0f)] 
        [SerializeField] protected float CurrentCooldownTimer;

        [Required]
        [SerializeField] protected TextMeshProUGUI CooldownDisplay;

        [SerializeField] protected Animator ButtonAnimator;

        public float CurrentCooldown => Mathf.Max(0f, CurrentCooldownTimer);

        public bool IsReady => CurrentCooldownTimer <= 0f;

        public ushort Id => SkillData.GetID();
        public string Name => SkillData.GetName();
        public string Tag => SkillData.GetSkillTag();
        void ISkill.Tick(float deltaTime) => this.Tick(deltaTime);


        // ==========================================
        // MonoBehaviour Life Cycle
        // ==========================================

        protected virtual void OnValidate()
        {
            if (!CooldownDisplay) CooldownDisplay = this.GetComponentInChildren<TextMeshProUGUI>();
        }

        protected virtual void Update()
        {
            UpdateCooldown();
        }

        private void UpdateCooldown()
        {
            if (CurrentCooldownTimer > 0f)
            {
                CurrentCooldownTimer -= Time.deltaTime;

                if (CurrentCooldownTimer <= 0f) CurrentCooldownTimer = 0f;

                CooldownDisplay.text = CurrentCooldownTimer == 0f ? string.Empty : $"{CurrentCooldownTimer:F1}";
            }
        }

        public void Execute(InputAction.CallbackContext context)
        {
            if (!IsReady) return;

            EffectSkill(context);
        }

        private void Tick(float deltaTime)
        {
            if (CurrentCooldownTimer > 0f)
            {
                CurrentCooldownTimer -= deltaTime;
            }
        }

        protected abstract void EffectSkill(InputAction.CallbackContext context);
    }
}