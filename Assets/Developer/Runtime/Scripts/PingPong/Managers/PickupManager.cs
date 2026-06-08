using UnityEngine;

using Developer.PingPong.Pickupable;
using UnityEngine.Pool;
using System.Collections;

namespace Developer.PingPong
{
    public class PickupManager : MonoBehaviour
    {
        public static PickupManager Instance { get; private set; }

        [SerializeField]
        private PickupableObject[] prefabs;
        [SerializeField]
        private float minPeriod;
        [SerializeField]
        private float maxPeriod;
        [SerializeField]
        private Vector2 padding;

        private Coroutine randomSpawnCoroutine;
        private PickupableObject currentRef;



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

        private void Start()
        {
            GameManager.EventAggregator.Subscribe<EndRoundEvent>(OnEndRoundEvent);
            GameManager.EventAggregator.Subscribe<ServeEvent>(OnServeEvent);

            StartNewSpawnCycle();
        }



        private void StartNewSpawnCycle()
        {
            if (randomSpawnCoroutine != null)
                StopCoroutine(randomSpawnCoroutine);

            randomSpawnCoroutine = StartCoroutine(RandomSpawnCoroutine());
        }

        private IEnumerator RandomSpawnCoroutine()
        {
            yield return new WaitForSeconds(Random.Range(minPeriod, maxPeriod));

            currentRef = Instantiate(prefabs[Random.Range(0, prefabs.Length)]);
            currentRef.transform.position = GetRandomSpawnPosition();
            currentRef.OnDestroyed += StartNewSpawnCycle;
        }

        private Vector2 GetRandomSpawnPosition()
        {
            Vector2 halfSizeValidArea = (GameManager.GameplaySettings.fieldSize - padding * 2f) * 0.5f;
            return new Vector2(
                Random.Range(-halfSizeValidArea.x, halfSizeValidArea.x),
                Random.Range(-halfSizeValidArea.y, halfSizeValidArea.y));
        }

        #region Callbacks
        private void OnEndRoundEvent(EndRoundEvent _Event)
        {
            if (randomSpawnCoroutine != null)
                StopCoroutine(randomSpawnCoroutine);

            if (currentRef != null)
                currentRef.OnDestroyed -= StartNewSpawnCycle;

            currentRef = null;
        } 

        private void OnServeEvent(ServeEvent _Event)
        {
            StartNewSpawnCycle();
        }
        #endregion
    }
}
