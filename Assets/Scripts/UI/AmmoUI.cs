using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private int maxAmmo = 30;
    [SerializeField] private int currentAmmo;
    [SerializeField] private int reserveAmmo = 90;
    [SerializeField] private float reloadTime = 2f;
    [SerializeField] private float shootCooldown = 0.1f;
    private bool isReloading = false;
    private float lastShootTime = 0f;
    private string ammoDisplayCache;

    private void Start()
    {
        currentAmmo = maxAmmo;
        UpdateAmmoDisplay();
    }

    private void Update()
    {
        // Check for right index trigger to shoot
        if (OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger) && !isReloading && Time.time >= lastShootTime + shootCooldown)
        {
            Shoot();
            lastShootTime = Time.time;
        }

        // Check for left index trigger to reload
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }

    private void Shoot()
    {
        if (currentAmmo > 0)
        {
            currentAmmo--;
            UpdateAmmoDisplay();
        }
        else
        {
            // Out of ammo - could add click sound here
            Debug.Log("Out of ammo! Reload!");
        }
    }

    private IEnumerator Reload()
    {
        if (currentAmmo >= maxAmmo || reserveAmmo <= 0)
        {
            yield break; // Already full or no reserve ammo
        }

        isReloading = true;
        ammoText.text = "RELOADING";
        
        yield return new WaitForSeconds(reloadTime);

        int ammoNeeded = maxAmmo - currentAmmo;
        int ammoToReload = Mathf.Min(ammoNeeded, reserveAmmo);
        
        currentAmmo += ammoToReload;
        reserveAmmo -= ammoToReload;
        
        isReloading = false;
        UpdateAmmoDisplay();
    }

    public void AddReserveAmmo(int amount)
    {
        reserveAmmo += amount;
        UpdateAmmoDisplay();
    }

    private void UpdateAmmoDisplay()
    {
        string newDisplay = currentAmmo.ToString() + "/" + maxAmmo.ToString();
        if (ammoDisplayCache != newDisplay)
        {
            ammoDisplayCache = newDisplay;
            ammoText.text = ammoDisplayCache;
        }
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetReserveAmmo()
    {
        return reserveAmmo;
    }
}
