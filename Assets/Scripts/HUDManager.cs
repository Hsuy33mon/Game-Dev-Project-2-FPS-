using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; set; }

    [Header("Ammo")]
    public TextMeshProUGUI magazineAmmoUI; // e.g. "30 / 30"
    public TextMeshProUGUI totalAmmoUI;    // e.g. "120"
    public Image ammoTypeUI;

    [Header("Weapons")]
    public Image activeWeaponUI;   // icon for the currently held weapon
    public Image unActiveWeaponUI; // optional: show previous/next/secondary

    [Header("Throwables")]
    public Image lethalUI;
    public TextMeshProUGUI lethalAmountUI;
    public Image tacticalUI;
    public TextMeshProUGUI tacticalAmmountUI;

    public GameObject middleDot;

    private void Awake()
    {
        if (Instance != null && Instance != this) Destroy(gameObject);
        else Instance = this;
    }

    // called by WeaponSwitcher when a new weapon is activated
    public void ApplyWeapon(Weapon w)
    {
        if (w == null) return;
        ApplyAmmo(w); // fill ammo texts too

        if (activeWeaponUI) activeWeaponUI.sprite = w.weaponIcon;
        if (ammoTypeUI)     ammoTypeUI.sprite   = w.ammoTypeIcon;
    }

    // called on shoot/reload/pickup via Weapon.OnAmmoChanged
    public void ApplyAmmo(Weapon w)
    {
        if (w == null) return;

        if (magazineAmmoUI) magazineAmmoUI.text = $"{w.bulletsLeft}";
        if (totalAmmoUI)    totalAmmoUI.text    =  $"{w.magazineSize}";
    }
}
