using TowerDefense.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_GameStartButton : MonoBehaviour
{
    Button _button;

    [Tooltip("픽셀 디졸브 전환 후 씬을 로드. 비워두면 즉시 씬을 로드.")]
    [SerializeField] UI_TransitionCanvas transitionCanvas;

    const string GamePlaySceneName = "GamePlayScene";

    private void Awake()
    {
        _button = GetComponent<Button>();

        _button.onClick.AddListener(OnStartGameClicked);
    }

    void OnStartGameClicked()
    {
        if (transitionCanvas != null)
        {
            transitionCanvas.PlayDissolveThenLoadScene(GamePlaySceneName);
        }
        else
        {
            SceneManager.LoadScene(GamePlaySceneName);
        }
    }
}
