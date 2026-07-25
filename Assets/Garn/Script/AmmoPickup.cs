using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int ammoAmount = 30;

    private void OnTriggerEnter(Collider other)
    {
        GunSystem gun = other.GetComponent<GunSystem>();

        if (gun != null)
        {
            gun.AddAmmo(ammoAmount);

            Destroy(gameObject);
        }
    }
}