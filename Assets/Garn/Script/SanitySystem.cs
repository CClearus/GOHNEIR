using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SanitySystem : MonoBehaviour
{
    public static SanitySystem Instance;

    [Header("UI")]
    public Image sanityBarFill;
    public TMP_Text sanityText;

    [Header("Sanity")]
    public float maxSanity = 200f;
    public float currentSanity = 200f;

    [Header("Passive Drain")]
    public float secondsPerPoint = 15f;

    private float timer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        currentSanity = maxSanity;
        UpdateUI();
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= secondsPerPoint)
        {
            timer = 0f;
            LoseSanity(1f);
        }
    }

    public void LoseSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity - amount, 0f, maxSanity);
        UpdateUI();

        if (currentSanity <= 0)
        {
            Debug.Log("Player has lost all sanity!");
            // TODO: Add game over or insanity effects here.
        }
    }

    public void GainSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity + amount, 0f, maxSanity);
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (sanityBarFill != null)
        {
            sanityBarFill.fillAmount = currentSanity / maxSanity;
        }

        if (sanityText != null)
        {
            sanityText.text = Mathf.RoundToInt(currentSanity).ToString();
        }
    }
}