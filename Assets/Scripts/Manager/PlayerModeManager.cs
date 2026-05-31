using UnityEngine;
using StarterAssets;
using System.Collections;

public class PlayerModeManager : MonoBehaviour
{
    public enum PlayerMode
    {
        TowerPlacementMode,
        HeroControlMode
    }

    public static PlayerModeManager Instance { get; private set; }

    public PlayerMode playerMode;

    StarterAssetsInputs heroInputs;
    public UI_ModeChangeButton modeChangeButton;

    public bool isModeChanging;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        heroInputs = Hero.Instance.GetComponent<StarterAssetsInputs>();
        Hero.Instance.OnDied += RequestPlayerModeChange;
        CompleteTowerPlacementMode();
    }

    public void RequestPlayerModeChange()
    {
        if (isModeChanging)
            return;

        StartCoroutine(ChangePlayerMode(playerMode));
    }

    IEnumerator ChangePlayerMode(PlayerMode currentMode)
    {
        isModeChanging = true;
        heroInputs.Initialize();

        // 모드 전환 시작
        switch (currentMode)
        {
            case PlayerMode.TowerPlacementMode:
                BeginHeroControlMode();
                break;
            case PlayerMode.HeroControlMode:
                BeginTowerPlacementMode();
                break;
        }

        // 블렌드가 시작될 때까지 한 프레임 대기
        yield return null;

        // 블렌드가 끝날 때까지 대기
        while (MainCameraController.Instance.IsBlending)
            yield return null;

        // 모드 전환 완료
        switch (currentMode)
        {
            case PlayerMode.TowerPlacementMode:
                CompleteHeroControlMode();
                break;
            case PlayerMode.HeroControlMode:
                CompleteTowerPlacementMode();
                break;
        }

        isModeChanging = false;
    }

    void BeginTowerPlacementMode()
    {
        MainCameraController.Instance.SetTowerPlacementModeView();

        // 이펙트 연출, Hero 모션 등 추가, UI 로직 변경
    }

    void CompleteTowerPlacementMode()
    {
        Hero.Instance.gameObject.SetActive(false);
        MouseManager.Instance.SetCursorLockState(false);
        GameUIManager.Instance.SetTowerPlacementMode();
        playerMode = PlayerMode.TowerPlacementMode;
    }

    void BeginHeroControlMode()
    {
        MainCameraController.Instance.SetHeroControlModeView();
    }

    void CompleteHeroControlMode()
    {
        Hero.Instance.gameObject.SetActive(true);
        MouseManager.Instance.SetCursorLockState(true);
        GameUIManager.Instance.SetHeroControlMode();
        playerMode = PlayerMode.HeroControlMode;
    }

    private void OnDestroy()
    {
        if (Hero.Instance != null)
            Hero.Instance.OnDied -= RequestPlayerModeChange;
    }
}
