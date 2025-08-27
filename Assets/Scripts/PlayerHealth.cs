using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    public AudioSource damageSource;
    public AudioClip damageSFX;

    public GameManager gameManager;
    
    void Start()
    {
        currentHealth = maxHealth;
        if (gameManager != null)
            gameManager.UpdateHealthUI(currentHealth);
    }

    public void TakeDamage()
    {
        currentHealth--;
        damageSource.PlayOneShot(damageSFX);

        if (gameManager != null)
            gameManager.UpdateHealthUI(currentHealth);

        if (currentHealth <= 0)
        {
            if (gameManager != null)
                gameManager.GameOver();
            Destroy(gameObject);
        }
    }

    public void RestoreFullHealth()
    {
        currentHealth = maxHealth;
        if (gameManager != null)
            gameManager.UpdateHealthUI(currentHealth);
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }
}