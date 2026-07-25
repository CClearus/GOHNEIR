using UnityEngine;
using TMPro;

public class GunSystem : MonoBehaviour
{
    [Header("Ammo")]
    public int currentAmmo = 300;

    [Header("UI")]
    public TMP_Text ammoText;

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (currentAmmo <= 0)
        {
            Debug.Log("Out of ammo!");
            return;
        }

        currentAmmo--;

        UpdateUI();

        Debug.Log("Bang!");
        // Later we'll spawn a bullet or use a raycast here.
    }

    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (ammoText != null)
        {
            ammoText.text = currentAmmo.ToString();
        }
    }
}