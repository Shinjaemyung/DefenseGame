using TMPro;
using UnityEngine;

public class UI_PlayerScorePanel : UI_Panel
{
    [SerializeField, Tooltip("점수 표시할 텍스트")]
    TextMeshProUGUI scoreText;

    private void Start()
    {
        GamePlayManager.Instance.PlayerScoreChanged += SetScoreDisplay;
        SetScoreDisplay(GamePlayManager.Instance.PlayerScore);
    }

    void SetScoreDisplay(int score)
    {
        scoreText.text = score.ToString();
    }
}
