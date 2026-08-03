using UnityEngine;

public class UI_ModeChangeEffectPanel : UI_Panel
{
    public void Initialize()
    {
        GameUIManager.Instance.OnBeginTowerPlacementMode += Show;
        GameUIManager.Instance.OnBeginHeroControlMode += Show;
        GameUIManager.Instance.OnCompleteTowerPlacementMode += Hide;
        GameUIManager.Instance.OnCompleteHeroControlMode += Hide;
    }

    private void OnDestroy()
    {
        GameUIManager.Instance.OnBeginTowerPlacementMode -= Show;
        GameUIManager.Instance.OnBeginHeroControlMode -= Show;
        GameUIManager.Instance.OnCompleteTowerPlacementMode -= Hide;
        GameUIManager.Instance.OnCompleteHeroControlMode -= Hide;
    }
}
