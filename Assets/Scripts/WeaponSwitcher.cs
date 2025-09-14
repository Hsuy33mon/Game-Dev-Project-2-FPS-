using UnityEngine;
using System.Collections.Generic;

public class WeaponSwitcher : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Parent under which all weapon objects live. Leave blank to search under this GameObject.")]
    public Transform weaponParent;

    [Header("State (Read Only)")]
    public int currentIndex = 0;
    public Weapon currentWeapon;
    public GrenadeWeapon currentGrenadeWeapon;

    private readonly List<GameObject> _weaponObjects = new List<GameObject>();
    private readonly List<Weapon> _weapons = new List<Weapon>();
    private readonly List<GrenadeWeapon> _grenadeWeapons = new List<GrenadeWeapon>();

    void Awake()
    {
        if (weaponParent == null) weaponParent = transform;

        // find all weapon objects under parent (inactive included)
        _weaponObjects.Clear();
        _weapons.Clear();
        _grenadeWeapons.Clear();

        // Get all child GameObjects
        for (int i = 0; i < weaponParent.childCount; i++)
        {
            GameObject child = weaponParent.GetChild(i).gameObject;
            _weaponObjects.Add(child);

            // Check if it's a regular weapon or grenade weapon
            Weapon weapon = child.GetComponent<Weapon>();
            GrenadeWeapon grenadeWeapon = child.GetComponent<GrenadeWeapon>();

            if (weapon != null) _weapons.Add(weapon);
            if (grenadeWeapon != null) _grenadeWeapons.Add(grenadeWeapon);
        }

        // disable all
        foreach (var obj in _weaponObjects) obj.SetActive(false);
    }

    void Start()
    {
        if (_weaponObjects.Count > 0)
        {
            SwitchTo(0); // start with first weapon
        }
        else
        {
            Debug.LogWarning("WeaponSwitcher: No weapon objects found under " + weaponParent.name);
        }
    }

    void Update()
    {
        if (_weaponObjects.Count <= 1) return;

        // mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) Next();
        else if (scroll < 0f) Prev();

        // number keys 1..9
        for (int i = 0; i < Mathf.Min(9, _weaponObjects.Count); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SwitchTo(i);
        }

        // quick toggle between last two
        if (Input.GetKeyDown(KeyCode.Q))
            SwitchTo((currentIndex + 1) % _weaponObjects.Count);
    }

    public void Next() => SwitchTo((currentIndex + 1) % _weaponObjects.Count);
    public void Prev() => SwitchTo((currentIndex - 1 + _weaponObjects.Count) % _weaponObjects.Count);

    public void SwitchTo(int index)
    {
        if (index < 0 || index >= _weaponObjects.Count) return;

        // unsubscribe from old weapon events
        if (currentWeapon != null)
            currentWeapon.OnAmmoChanged -= HandleAmmoChanged;
        if (currentGrenadeWeapon != null)
            currentGrenadeWeapon.OnAmmoChanged -= HandleGrenadeAmmoChanged;

        // deactivate old
        if (currentIndex >= 0 && currentIndex < _weaponObjects.Count)
            _weaponObjects[currentIndex].SetActive(false);

        // activate new
        currentIndex = index;
        GameObject newWeaponObj = _weaponObjects[index];
        newWeaponObj.SetActive(true);

        // determine if it's a regular weapon or grenade weapon
        currentWeapon = newWeaponObj.GetComponent<Weapon>();
        currentGrenadeWeapon = newWeaponObj.GetComponent<GrenadeWeapon>();

        // subscribe to events
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged += HandleAmmoChanged;
            // refresh HUD immediately for regular weapons
            HUDManager.Instance.ApplyWeapon(currentWeapon);
        }

        if (currentGrenadeWeapon != null)
        {
            currentGrenadeWeapon.OnAmmoChanged += HandleGrenadeAmmoChanged;
            // refresh HUD immediately for grenade weapons
            HUDManager.Instance.ApplyGrenadeWeapon(currentGrenadeWeapon);
        }
    }

    private void HandleAmmoChanged(Weapon w)
    {
        // live HUD update when you shoot/reload/pickup
        if (HUDManager.Instance != null)
            HUDManager.Instance.ApplyAmmo(w);
    }

    private void HandleGrenadeAmmoChanged(GrenadeWeapon gw)
    {
        // live HUD update when you throw grenades/restock
        if (HUDManager.Instance != null)
            HUDManager.Instance.ApplyGrenadeAmmo(gw);
    }

    // OPTIONAL helpers you can call from pickups/inventory
    public Weapon GetCurrent() => currentWeapon;
    public GrenadeWeapon GetCurrentGrenade() => currentGrenadeWeapon;
    public List<Weapon> GetAll() => _weapons;
    public List<GrenadeWeapon> GetAllGrenades() => _grenadeWeapons;
    public List<GameObject> GetAllWeaponObjects() => _weaponObjects;
}
