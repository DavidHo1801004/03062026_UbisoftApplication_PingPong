using Developer.Collections.Events;
using Developer.GameplaySystems;
using Developer.General.Managers;
using Developer.General.Settings;
using System.Collections;

using UnityEngine;

namespace Developer.PingPong
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }



        [SerializeField]
        private DisplaySettings displaySettings;
        [SerializeField]
        private GameplaySettings gameplaySettings;

        public static DisplaySettings DisplaySettings => Instance.displaySettings;

        public static GameplaySettings GameplaySettings => Instance.gameplaySettings;

        private readonly EventAggregator eventAggregator = new();
        public static EventAggregator EventAggregator => Instance.eventAggregator;



        private void Awake()
        {
            #region Singleton
            if (!Instance)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            #endregion
        }

        private void OnEnable()
        {
            eventAggregator.Subscribe<GoalEvent>(OnGoalEvent);
        }

        private void OnDisable()
        {
            eventAggregator.Unsubscribe<GoalEvent>(OnGoalEvent);
        }



        private void OnGoalEvent(GoalEvent _Event)
        {
            UnitTag leadTeam = _Event.GoalData.ContainTags(UnitTag.TeamA) ? UnitTag.TeamA : UnitTag.TeamB;
            StartCoroutine(StartNewRoundCoroutine(new EndRoundEvent(leadTeam)));
        }

        private IEnumerator StartNewRoundCoroutine(EndRoundEvent _Event)
        {
            yield return new WaitForSeconds(1f);

            eventAggregator.Publish(_Event);
        }
    }

    public class EndRoundEvent
    {
        public readonly UnitTag nextServeTeamTag;

        public EndRoundEvent(UnitTag _LeadTeamTag)
        {
            nextServeTeamTag = _LeadTeamTag;
        }
    }

    public class GameOverEvent
    {
        public readonly string Message;

        public GameOverEvent(string _Message)
        {
            Message = _Message;
        }
    }
}
