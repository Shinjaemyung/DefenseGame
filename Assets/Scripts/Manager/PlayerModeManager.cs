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
        SetTowerPlacementMode();
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
        yield return new WaitForSeconds(1.5f);

        switch (currentMode)
        {
            case PlayerMode.TowerPlacementMode:
                SetHeroControlMode();
                break;
            case PlayerMode.HeroControlMode:
                SetTowerPlacementMode();
                break;
        }

        isModeChanging = false;
    }

    void SetTowerPlacementMode()
    {
        Hero.Instance.gameObject.SetActive(false);
        MouseManager.Instance.SetCursorLockState(false);
        MainCameraController.Instance.SetTowerPlacementModeView();
        GameUIManager.Instance.SetTowerPlacementModeUI();
        playerMode = PlayerMode.TowerPlacementMode;
    }

    void SetHeroControlMode()
    {
        Hero.Instance.gameObject.SetActive(true);
        MouseManager.Instance.SetCursorLockState(true);
        MainCameraController.Instance.SetHeroControlModeView();
        GameUIManager.Instance.SetHeroControlModeUI();
        playerMode = PlayerMode.HeroControlMode;
    }

    private void OnDestroy()
    {
        if (Hero.Instance != null)
            Hero.Instance.OnDied -= RequestPlayerModeChange;
    }
}
