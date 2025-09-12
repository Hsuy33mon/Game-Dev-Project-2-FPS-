using UnityEngine;
using System.Collections;
using System;                 // <-- for Action

public class Weapon : MonoBehaviour
{
    // ---- HUD / Inventory metadata ----
    [Header("HUD / Inventory")]
    public string weaponName = "M16A4";
    public Sprite weaponIcon;         // shown on HUD as active weapon
    public Sprite ammoTypeIcon;       // e.g., 5.56 / 9mm icon

    // ---- Shooting ----
    [Header("Shooting")]
    public bool isShooting, readyToShoot = true;
    private bool allowReset = true;
    public float shootingDelay = 2f;

    // ---- Burst ----
    [Header("Burst")]
    public int bulletsPerBurst = 3;
    [HideInInspector] public int burstBulletsLeft;

    // ---- Spread ----
    [Header("Spread")]
    public float spreadIntensity;
    public float hipSpreadIntensity;
    public float adsSpreadIntensity;

    // ---- Bullet ----
    [Header("Projectile")]
    public GameObject bulletPrefab;
    public Transform bulletSpawn;
    public float bulletVelocity = 30;
    public float bulletPrefabLifeTime = 3f;

    [Header("FX")]
    public GameObject muzzleEffect;
    private Animator animator;

    // ---- Ammo / Reload ----
    [Header("Ammo / Reload")]
    public float reloadTime = 2f;
    public int magazineSize = 30;
    public int bulletsLeft = 30;   // in-mag
    public int totalAmmo = 120;    // reserve
    public bool isReloading;

    public enum WeaponModel { Pistol1911, M4_8, M16a4 }
    public WeaponModel thisWeaponModel;

    public enum ShootingMode { Single, Burst, Auto }
    public ShootingMode currentShootingMode = ShootingMode.Auto;

    // ---- Events ----
    // Subscribe from HUD/WeaponSwitcher to get live updates.
    public event Action<Weapon> OnAmmoChanged;
    bool isAds;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        readyToShoot = true;
        burstBulletsLeft = bulletsPerBurst;

        // make sure starting mag is valid
        bulletsLeft = Mathf.Clamp(bulletsLeft, 0, magazineSize);
        spreadIntensity = hipSpreadIntensity;
    }

    private void OnEnable()
    {
        // when weapon becomes active, push current ammo to HUD
        NotifyAmmoChanged();
    }

    void Update()
    {

        // --- ADS input (hold to aim) ---
        if (Input.GetMouseButtonDown(1))
        {
            EnterADS();
        }
        if (Input.GetMouseButtonUp(1))
        {
            ExitADS();
        }

        // input -> fire mode
        if (currentShootingMode == ShootingMode.Auto)
            isShooting = Input.GetKey(KeyCode.Mouse0);
        else
            isShooting = Input.GetKeyDown(KeyCode.Mouse0);

        // dry-fire sound
        if (bulletsLeft == 0 && isShooting && !isReloading)
        {
            SoundManager.Instance.emptyMagazineSoundM1911.Play();
        }

        // manual reload
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && bulletsLeft < magazineSize && totalAmmo > 0)
        {
            Reload();
        }

        // auto reload when idle & empty (and have reserves)
        if (readyToShoot && !isShooting && !isReloading && bulletsLeft <= 0 && totalAmmo > 0)
        {
            Reload();
        }

        // fire
        if (readyToShoot && isShooting && bulletsLeft > 0 && !isReloading)
        {
            burstBulletsLeft = bulletsPerBurst;
            FireWeapon();
        }
    }

    private void EnterADS()
    {
        animator.SetTrigger("enterAds");
        isAds = true;
        HUDManager.Instance.middleDot.SetActive(false);
        spreadIntensity = adsSpreadIntensity;
    }

    private void ExitADS()
    {
        animator.SetTrigger("exitAds");
        isAds = false;
        HUDManager.Instance.middleDot.SetActive(true);
        spreadIntensity = hipSpreadIntensity;
    }

    private void FireWeapon()
    {
        bulletsLeft--;
        NotifyAmmoChanged();

        if (muzzleEffect) muzzleEffect.GetComponent<ParticleSystem>().Play();

        if (isAds)
        {
            animator.SetTrigger("RECOIL_ADS");
        }
        else
        {
            animator.SetTrigger("RECOIL");
        }

        SoundManager.Instance.PlayShootingSound(thisWeaponModel);

        readyToShoot = false;

        Vector3 shootingDirection = CalculateDirectionAndSpread().normalized;

        // spawn bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawn.position, Quaternion.identity);
        bullet.transform.forward = shootingDirection;
        bullet.GetComponent<Rigidbody>().AddForce(bulletSpawn.forward.normalized * bulletVelocity, ForceMode.Impulse);
        StartCoroutine(DestroyBulletAfterTime(bullet, bulletPrefabLifeTime));

        // reset shot gate
        if (allowReset)
        {
            Invoke(nameof(ResetShot), shootingDelay);
            allowReset = false;
        }

        // burst scheduling
        if (currentShootingMode == ShootingMode.Burst && burstBulletsLeft > 1)
        {
            burstBulletsLeft--;
            Invoke(nameof(FireWeapon), shootingDelay);
        }
    }

    public void Reload()
    {
        if (isReloading) return;
        if (bulletsLeft >= magazineSize) return;
        if (totalAmmo <= 0) return;

        SoundManager.Instance.PlayReloadSound(thisWeaponModel);
        if (animator) animator.SetTrigger("RELOAD");
        isReloading = true;
        Invoke(nameof(ReloadCompleted), reloadTime);
    }

    private void ReloadCompleted()
    {
        // pull from reserve into magazine
        int need = magazineSize - bulletsLeft;
        int toLoad = Mathf.Min(need, totalAmmo);
        bulletsLeft += toLoad;
        totalAmmo   -= toLoad;

        isReloading = false;
        NotifyAmmoChanged();
    }

    private void ResetShot()
    {
        readyToShoot = true;
        allowReset = true;
    }

    public Vector3 CalculateDirectionAndSpread()
    {
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        Vector3 targetPoint = Physics.Raycast(ray, out RaycastHit hit) ? hit.point : ray.GetPoint(100);
        Vector3 direction = targetPoint - bulletSpawn.position;

        float z = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);
        float y = UnityEngine.Random.Range(-spreadIntensity, spreadIntensity);

        return direction + new Vector3(0, y, z);
    }

    private IEnumerator DestroyBulletAfterTime(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(bullet);
    }

    // ---- Public helpers ----
    public void AddReserveAmmo(int amount)
    {
        totalAmmo = Mathf.Max(0, totalAmmo + amount);
        NotifyAmmoChanged();
    }

    public void NotifyAmmoChanged()
    {
        OnAmmoChanged?.Invoke(this);
    }
}
