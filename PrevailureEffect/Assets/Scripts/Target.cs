using UnityEngine;
using UnityEngine.UI;

public class Target : MonoBehaviour
{
	//Adding enemy health UI
    public float startingHealth = 50f;
	public float currentHealth;

	public Image healthBar;

	void Awake()
	{
		currentHealth = startingHealth;
	}

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
		healthBar.fillAmount = currentHealth / startingHealth;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
	{
        Destroy(gameObject);
    }

 }


