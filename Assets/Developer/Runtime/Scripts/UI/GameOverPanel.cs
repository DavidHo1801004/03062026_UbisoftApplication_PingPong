using Developer.PingPong;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Developer.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        [SerializeField]
        private Transform mainPanel;
        [SerializeField]
        private TextMeshProUGUI text;



        private void OnEnable()
        {
            GameManager.EventAggregator.Subscribe<GameOverEvent>(OnGameOverEvent);
        }

        private void OnDisable()
        {
            GameManager.EventAggregator.Unsubscribe<GameOverEvent>(OnGameOverEvent);
        }

        private void Start()
        {
            mainPanel.gameObject.SetActive(false);
        }



        private void OnGameOverEvent(GameOverEvent _Event)
        {
            text.text = _Event.Message;
            mainPanel.gameObject.SetActive(true);
        }

        public void ResetLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
