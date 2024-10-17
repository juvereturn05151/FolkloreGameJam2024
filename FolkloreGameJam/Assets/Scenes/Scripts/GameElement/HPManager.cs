using UnityEngine;
using System;
using MoreMountains.Feedbacks;

public class HPManager : MonoBehaviour
{
    public static HPManager Instance { get; private set; }

    public event Action<int> OnHealthChanged; // Event to notify UI of health changes

    private int maxHealth = 4; // Max HP
    private int currentHealth;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentHealth = maxHealth; // Initialize current health
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Method to take damage
    public void TakeDamage(int damage)
    {
        SoundManager.instance.PlaySFX("SFX_PlayerLoseHP");
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health does not go below 0
        OnHealthChanged?.Invoke(currentHealth); // Notify UI of health change
        
        GameplayUIManager.Instance.OnGhostAnger?.Invoke();
        if (currentHealth <= 0)
        {
            GameplayUIManager.Instance.OnGameOver();
        }
    }

    // Method to heal the player
    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health does not exceed max
        OnHealthChanged?.Invoke(currentHealth); // Notify UI of health change
    }

    // Method to get current health
    public int GetCurrentHealth()
    {
        return currentHealth;
    }
}