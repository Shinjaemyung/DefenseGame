using UnityEngine;

public class TestManager : MonoBehaviour
{


    HeroAppearanceChanger heroAppearanceChanger;

    private void Awake()
    {
        heroAppearanceChanger = FindAnyObjectByType<HeroAppearanceChanger>();
    }

    void Update()
    {
#if UNITY_EDITOR
        Test();
#endif

        if (Input.GetKeyDown(KeyCode.F1))
            EnvironmentManager.Instance.SetEnvironment(EnvironmentManager.Instance.presets[0].id);

        if (Input.GetKeyDown(KeyCode.F2))
            EnvironmentManager.Instance.SetEnvironment(EnvironmentManager.Instance.presets[1].id);

        if (Input.GetKeyDown(KeyCode.F3))
            EnvironmentManager.Instance.SetEnvironment(EnvironmentManager.Instance.presets[2].id);

        if (Input.GetKeyDown(KeyCode.Alpha1))
            heroAppearanceChanger.ApplySet(0);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            heroAppearanceChanger.ApplySet(1);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            heroAppearanceChanger.ApplySet(2);
    }

    void Test()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            GamePlayManager.Instance.UpdatePlayerHealth(-10);

        if (Input.GetKeyDown(KeyCode.X))
            GamePlayManager.Instance.UpdatePlayerGold(100);

        if (Input.GetKeyDown(KeyCode.P))
            Hero.Instance.UpdateHealth(-50);
    }

}
