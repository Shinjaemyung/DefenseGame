using UnityEngine;
using StarterAssets;
using System.Collections;
using TowerDefense.UI;

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


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        heroInputs = Hero.Instance.GetComponent<StarterAssetsInputs>();
        Hero.Instance.Died += RequestPlayerModeChange;
        CompleteTowerPlacementMode();
    }

    public void RequestPlayerModeChange()
    {
        if (CameraManager.Instance.IsBlending)
            return;

        if (playerMode == PlayerMode.TowerPlacementMode && Hero.Instance.isDead)
            return;

        StartCoroutine(ChangePlayerMode(playerMode));
    }

    IEnumerator ChangePlayerMode(PlayerMode currentMode)
    {
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
        while (CameraManager.Instance.IsBlending)
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
    }

    void BeginTowerPlacementMode()
    {
        CameraManager.Instance.SetTowerPlacementModeView();

        // 이펙트 연출, Hero 모션 등 추가
        GameUIManager.Instance.BeginTowerPlacementMode();
    }

    void CompleteTowerPlacementMode()
    {
        GameUIManager.Instance.CompleteTowerPlacementMode();
        Hero.Instance.OnHeroDeactivated();
        MouseManager.Instance.SetCursorLockState(false);
        playerMode = PlayerMode.TowerPlacementMode;
    }

    void BeginHeroControlMode()
    {
        PlacementManager.Instance.CancelPlacementState();
        CameraManager.Instance.SetHeroControlModeView();
        GameUIManager.Instance.BeginHeroControlMode();
        RadiusVisualizerController.Instance.HideRadiusVisualizers();
        MouseManager.Instance.SetCursorLockState(true);
    }

    void CompleteHeroControlMode()
    {
        GameUIManager.Instance.CompleteHeroControlMode();
        Hero.Instance.OnHeroActivated();
        playerMode = PlayerMode.HeroControlMode;
    }

    private void OnDestroy()
    {
        if (Hero.Instance != null)
            Hero.Instance.Died -= RequestPlayerModeChange;
    }
}
