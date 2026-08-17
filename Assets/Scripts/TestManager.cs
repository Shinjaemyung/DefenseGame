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

        if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            EnvironmentManager.Instance.SetEnvironment(EnvironmentManager.Instance.presets[0].id);

        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            EnvironmentManager.Instance.SetEnvironment(EnvironmentManager.Instance.presets[1].id);

        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            EnvironmentManager.Instance.SetEnvironment(EnvironmentManager.Instance.presets[2].id);

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            heroAppearanceChanger.ApplySet(0);

        if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            heroAppearanceChanger.ApplySet(1);

        if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
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
