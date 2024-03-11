using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth;
    public int currentHealth;

    public float currentHealthPercentage { get
        {
            return (float)currentHealth/(float)maxHealth;
        } }

    private Character character;

    private void Awake()
    {
        character = GetComponent<Character>();
        currentHealth = maxHealth;
    }

    public void ApplyDamage(int damage)
    {
        currentHealth -= damage;

        CheckHealth();
    }
    public void CheckHealth()
    {
        if (currentHealth <= 0)
        {
            character.SetCharacterState(Character.characterState.deathState);
        }
    }

    public void AddHealth(int health)
    {
        currentHealth += health;
        if(currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
}
