using UnityEngine;
using System.Collections;
using System;

public class GrenadeWeapon : MonoBehaviour
{
    // ---- HUD / Inventory metadata ----
    [Header("HUD / Inventory")]
    public string weaponName = "Grenade";
    public Sprite weaponIcon;         // shown on HUD as active weapon
    public Sprite ammoTypeIcon;       // grenade icon

    // ---- Throwing ----
    [Header("Throwing")]
    public bool isThrowing, readyToThrow = true;
    private bool allowReset = true;
    public float throwingDelay = 1f;

    // ---- Grenade ----
    [Header("Grenade")]
    public GameObject grenadePrefab;        // The grenade that gets thrown
    public Transform grenadeSpawn;          // Where to spawn thrown grenades
    public GameObject grenadeViewModel;     // The grenade model visible in player's hand
    public float throwForce = 40f;
    public float throwUpwardForce = 20f;

    [Header("FX")]
    private Animator animator;

    // ---- Ammo ----
    [Header("Ammo")]
    public int grenadesLeft = 3;   // grenades in hand
    public int totalGrenades = 9;    // reserve grenades

    // ---- Events ----
    // Subscribe from HUD/WeaponSwitcher to get live updates.
    public event Action<GrenadeWeapon> OnAmmoChanged;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        readyToThrow = true;

        // make sure starting grenades is valid
        grenadesLeft = Mathf.Clamp(grenadesLeft, 0, 10); // max 10 grenades at once
    }

    private void OnEnable()
    {
        // when weapon becomes active, push current ammo to HUD
        NotifyAmmoChanged();

        // Show grenade model when weapon becomes active
        if (grenadeViewModel != null)
        {
            grenadeViewModel.SetActive(true);
        }
    }

    private void OnDisable()
    {
        // Hide grenade model when weapon becomes inactive
        if (grenadeViewModel != null)
        {
            grenadeViewModel.SetActive(false);
        }
    }

    void Update()
    {
        // input -> throw
        isThrowing = Input.GetKeyDown(KeyCode.Mouse0);

        // dry-throw (no grenades left)
        if (grenadesLeft == 0 && isThrowing)
        {
            // Play empty sound or just don't throw
            Debug.Log("No grenades left!");
        }

        // throw grenade
        if (readyToThrow && isThrowing && grenadesLeft > 0)
        {
            ThrowGrenade();
        }

        // reload/restock grenades
        if (Input.GetKeyDown(KeyCode.R) && grenadesLeft < 10 && totalGrenades > 0)
        {
            RestockGrenades();
        }
    }

    private void ThrowGrenade()
    {
        if (grenadesLeft <= 0) return;

        grenadesLeft--;
        NotifyAmmoChanged();

        // Play throwing animation if available
        if (animator)
        {
            animator.SetTrigger("THROW");
        }

        readyToThrow = false;

        // Hide the grenade model temporarily during throw
        if (grenadeViewModel != null)
        {
            grenadeViewModel.SetActive(false);
        }

        // Delay the actual throw to sync with animation
        Invoke(nameof(ActuallyThrowGrenade), 0.3f); // Adjust timing as needed

        // Reset throw gate
        if (allowReset)
        {
            Invoke(nameof(ResetThrow), throwingDelay);
            allowReset = false;
        }
    }

    private void ActuallyThrowGrenade()
    {
        // Calculate throwing direction
        Vector3 throwingDirection = CalculateThrowDirection();

        // Spawn grenade
        GameObject grenade = Instantiate(grenadePrefab, grenadeSpawn.position, Quaternion.identity);

        // Set up the grenade
        Rigidbody grenadeRb = grenade.GetComponent<Rigidbody>();
        Throwable throwableComponent = grenade.GetComponent<Throwable>();

        if (grenadeRb != null)
        {
            Vector3 forceToAdd = throwingDirection * throwForce + Vector3.up * throwUpwardForce;
            grenadeRb.AddForce(forceToAdd, ForceMode.Impulse);
        }

        if (throwableComponent != null)
        {
            throwableComponent.hasBeenThrown = true;
        }

        // Show the grenade model again after a short delay if we have more grenades
        if (grenadesLeft > 0)
        {
            Invoke(nameof(ShowGrenadeModel), 0.5f);
        }
    }

    private void ShowGrenadeModel()
    {
        if (grenadeViewModel != null && grenadesLeft > 0)
        {
            grenadeViewModel.SetActive(true);
        }
    }

    private Vector3 CalculateThrowDirection()
    {
        // Get direction from camera center
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
        }

        Vector3 direction = (targetPoint - grenadeSpawn.position).normalized;
        return direction;
    }

    public void RestockGrenades()
    {
        if (totalGrenades <= 0) return;

        // Add grenades from reserve to hand
        int need = 10 - grenadesLeft; // max 10 grenades in hand
        int toAdd = Mathf.Min(need, totalGrenades);
        grenadesLeft += toAdd;
        totalGrenades -= toAdd;

        NotifyAmmoChanged();

        // Show grenade model if we now have grenades and weapon is active
        if (grenadesLeft > 0 && grenadeViewModel != null && gameObject.activeInHierarchy)
        {
            grenadeViewModel.SetActive(true);
        }

        Debug.Log($"Restocked grenades. In hand: {grenadesLeft}, Reserve: {totalGrenades}");
    }

    private void ResetThrow()
    {
        readyToThrow = true;
        allowReset = true;

        // Ensure grenade model is visible if we have grenades
        if (grenadesLeft > 0 && grenadeViewModel != null && gameObject.activeInHierarchy)
        {
            grenadeViewModel.SetActive(true);
        }
    }

    // ---- Public helpers ----
    public void AddGrenades(int amount)
    {
        totalGrenades = Mathf.Max(0, totalGrenades + amount);
        NotifyAmmoChanged();
    }

    public void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(this);
    }

    // Methods to make it compatible with WeaponSwitcher expectations
    public int GetCurrentAmmo() => grenadesLeft;
    public int GetTotalAmmo() => totalGrenades;
    public string GetWeaponName() => weaponName;
    public Sprite GetWeaponIcon() => weaponIcon;
    public Sprite GetAmmoTypeIcon() => ammoTypeIcon;
}