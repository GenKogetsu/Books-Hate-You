using Kogetsu.Library.Attribute;
using Kogetsu.Library.DesignPatternCore;

namespace Kogetsu.Library.Core
{
    [CreateHierarchyMenu("KogetsuLibrary/Core/Manager")]
    public class GameManager : Singleton<GameManager>
    {
        [ReadOnly]
        [SerializeField] protected GameState CurrentGameState;

        public void SetGameState(GameState state)
        {
            if (!EventBus.Instance)
            {
                Debug.LogError("<color=red><b>[EventBus]</b></color> <color=yellow>Instance is null!</color> Please ensure an EventBus exists in the scene.");
                return;
            }

            CurrentGameState = state;
            EventBus.Instance.Publish(new GameStateEvent(CurrentGameState));
        }

        public bool IsPasue() => CurrentGameState == GameState.Pause;
    }
}