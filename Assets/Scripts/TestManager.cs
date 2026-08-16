using UnityEngine;

public class TestManager : MonoBehaviour
{
#if UNITY_EDITOR

    void Update()
    {
        Test_ChangeHealthAndGold();
    }

    void Test_ChangeHealthAndGold()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            GamePlayManager.Instance.UpdatePlayerHealth(-10);

        if (Input.GetKeyDown(KeyCode.X))
            GamePlayManager.Instance.UpdatePlayerGold(100);

        if (Input.GetKeyDown(KeyCode.P))
            Hero.Instance.UpdateHealth(-50);

        if (Input.GetKeyDown(KeyCode.Alpha1))
            EnvironmentManager.Instance.SetEnvironment(EnvironmentManager.Instance.presets[0].id);

        if (Input.GetKeyDown(KeyCode.Alpha2))
            EnvironmentManager.Instance.SetEnvironment(EnvironmentManager.Instance.presets[1].id);

        if (Input.GetKeyDown(KeyCode.Alpha3))
            EnvironmentManager.Instance.SetEnvironment(EnvironmentManager.Instance.presets[2].id);
    }
#endif
}
