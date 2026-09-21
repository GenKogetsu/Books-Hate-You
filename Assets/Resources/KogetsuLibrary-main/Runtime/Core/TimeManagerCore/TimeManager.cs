using System;
using System.Text;
using System.Globalization;
using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    [CreateHierarchyMenu("KogetsuLibrary/Core/Manager")]
    public class TimeManager : Singleton<TimeManager>
    {

        [SerializeField] protected TimeScaleDataSO ScaleData;

        [field: Header("Calendar")]
        [field: SerializeField, ReadOnly]
        public string LocalGameTimeDisplay { get; protected set; }
        public DateTime LocalGameTime { get; protected set; } = new(hour: 08, minute: 00, second: 00, day: 01, month: 01, year: 2026);

        [Header("TimerScale")]
        [ReadOnly]
        [SerializeField] protected double TimePerFrame;

        [field: SerializeField, ReadOnly]
        public double CurrentTimeScale { get; protected set; }

        [field: SerializeField]
        public double AcceleratorScale { get; protected set; }

        protected double CacheAcceleratorScale;

        [field: SerializeField]
        public double AccelerationMultiplier { get; protected set; }

        public DateTime RunningTime { get; protected set; }
        public DateTime RunningTimeGameTime { get; protected set; }

        protected StringBuilder TimeBuilder = new();

        [Header("Debug")]
        [ReadOnly]
        [SerializeField] protected string RunningTimeDisplay = $"0s 0ms";

        [ReadOnly]
        [SerializeField] protected string RunningGameTimeDisplay = $"0s 0ms";

        protected virtual void OnEnable()
        {
            if (EventBus.Instance)
            {
                EventBus.Instance.Subscribe<GameStateEvent>(PauseAcceleratorScale);
            }
        }

        protected virtual void OnDisable()
        {
            if (EventBus.Instance)
            {
                EventBus.Instance.Unsubscribe<GameStateEvent>(PauseAcceleratorScale);
            }
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            CalculateAcceleratorScale();

            if (Application.isPlaying) return;

            SetUp();
        }
#endif

        protected virtual void Start()
        {
            SetUp();
        }

        protected virtual void LateUpdate()
        {
            TimePerFrame = CurrentTimeScale * Time.deltaTime;

            UpdateDateTime();
        }

        protected virtual void CalculateAcceleratorScale()
        {
            CurrentTimeScale = AcceleratorScale * AccelerationMultiplier;
        }

        protected virtual void UpdateDateTime()
        {
            RunningTime = RunningTime.AddSeconds(Time.deltaTime);
            RunningTimeGameTime = RunningTimeGameTime.AddSeconds(TimePerFrame);
            LocalGameTime = LocalGameTime.AddSeconds(TimePerFrame);

            UpdateDateTimer(ref RunningTimeDisplay, RunningTime);
            UpdateDateTimer(ref RunningGameTimeDisplay, RunningTimeGameTime);
            LocalGameTimeDisplay = LocalGameTime.ToString("HH:mm:ss , ddd dd MMM yyyy", CultureInfo.InvariantCulture);
        }

        protected virtual void SetUp()
        {
            LocalGameTimeDisplay = LocalGameTime.ToString("HH:mm:ss , ddd dd MMM yyyy", CultureInfo.InvariantCulture);
            AcceleratorScale = ScaleData.BaseAcceleratorScale;
            AccelerationMultiplier = ScaleData.BaseAccelerationMultiplier;

            CalculateAcceleratorScale();
        }

        protected virtual void PauseAcceleratorScale(GameStateEvent eventDate)
        {
            if (eventDate.State == GameState.Pause)
            {
                if (CacheAcceleratorScale != 0f) return;

                CacheAcceleratorScale = AcceleratorScale;
                AcceleratorScale = 0f;
            }

            else
            {
                AcceleratorScale = CacheAcceleratorScale;
                CacheAcceleratorScale = 0f;
            }


            CalculateAcceleratorScale();
        }

        protected virtual void UpdateDateTimer(ref string display, DateTime dateTime)
        {

            TimeBuilder.Clear();

            if (dateTime.Year > 1)
            {
                TimeBuilder.Append($"{dateTime.Year - 1}y ");
            }

            if (dateTime.Month > 1)
            {
                TimeBuilder.Append($"{dateTime.Month - 1}mo ");
            }

            if (dateTime.Day > 1)
            {
                TimeBuilder.Append($"{dateTime.Day - 1}d ");
            }

            if (dateTime.Hour > 0)
            {
                TimeBuilder.Append($"{dateTime.Hour}h ");
            }

            if (dateTime.Minute > 0)
            {
                TimeBuilder.Append($"{dateTime.Minute}m ");
            }

            if (dateTime.Second > 0 || TimeBuilder.Length == 0)
            {
                TimeBuilder.Append($"{dateTime.Second}s ");
            }

            if (dateTime.Millisecond > 0 || TimeBuilder.Length == 0)
            {
                TimeBuilder.Append($"{dateTime.Millisecond}ms");
            }

            display = TimeBuilder.ToString().Trim();
        }

        public virtual void SetAccelerationMultiplier(float multiplier)
        {
            AccelerationMultiplier = multiplier;
        }
    }
}