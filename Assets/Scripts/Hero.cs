using UnityEngine;

public class Hero
{

    float health = 100;
    bool isDead;

    public void UpdateHealth(float amount)
    {
        health += amount;
    }

    public void Die()
    {
        isDead = true;
    }
}
