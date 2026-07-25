using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class HealthDiddy : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("UI")]
    public Image healthBar;
    public TMP_Text healthText;

    [Header("Death Fade")]
    public Image blackScreen;
    public float delayBeforeFade = 2f;
    public float fadeDuration = 2f;

    [Header("Scene")]
    public string gameOverSceneName = "GameOver";

    private bool isDead = false;

    void Start()
    {
        Time.timeScale = 1f;

        currentHealth = maxHealth;
        UpdateHealthUI();

        if (blackScreen != null)
        {
            Color c = blackScreen.color;
            c.a = 0f;
            blackScreen.color = c;
        }
    }

    void Update()
    {
        // TEST DAMAGE
        if (Input.GetKeyDown(KeyCode.Z))
        {
            TakeDamage(20);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();

        Debug.Log("Health: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = currentHealth / maxHealth;
        }

        if (healthText != null)
        {
            healthText.text = Mathf.RoundToInt(currentHealth).ToString();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        Debug.Log("Player Died!");

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        // Freeze the game
        Time.timeScale = 0f;

        // Wait 2 REAL seconds
        yield return new WaitForSecondsRealtime(delayBeforeFade);

        // Fade to black
        if (blackScreen != null)
        {
            Color color = blackScreen.color;
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;

                color.a = Mathf.Lerp(0f, 1f, timer / fadeDuration);
                blackScreen.color = color;

                yield return null;
            }

            color.a = 1f;
            blackScreen.color = color;
        }

        // Resume time before loading the next scene
        Time.timeScale = 1f;

        // Load your chosen scene
        SceneManager.LoadScene(gameOverSceneName);
    }
}