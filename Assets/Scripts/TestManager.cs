using UnityEngine;

public class TestManager : MonoBehaviour
{
    void Update()
    {
        Test_ChangeHealthAndGold();
    }

    void Test_ChangeHealthAndGold()
    {
        if (Input.GetKeyDown(KeyCode.Z))
            GameManager.Instance.UpdatePlayerHealth(-10);

        if (Input.GetKeyDown(KeyCode.X))
            GameManager.Instance.UpdatePlayerGold(10);
    }

}
