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

    private readonly List<Weapon> _weapons = new List<Weapon>();

    void Awake()
    {
        if (weaponParent == null) weaponParent = transform;

        // find all weapons under parent (inactive included)
        var all = weaponParent.GetComponentsInChildren<Weapon>(true);
        _weapons.Clear();
        _weapons.AddRange(all);

        // disable all
        foreach (var w in _weapons) w.gameObject.SetActive(false);
    }

    void Start()
    {
        if (_weapons.Count > 0)
        {
            SwitchTo(0); // start with first weapon
        }
        else
        {
            Debug.LogWarning("WeaponSwitcher: No weapons found under " + weaponParent.name);
        }
    }

    void Update()
    {
        if (_weapons.Count <= 1) return;

        // mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) Next();
        else if (scroll < 0f) Prev();

        // number keys 1..9
        for (int i = 0; i < Mathf.Min(9, _weapons.Count); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SwitchTo(i);
        }

        // quick toggle between last two
        if (Input.GetKeyDown(KeyCode.Q))
            SwitchTo((currentIndex + 1) % _weapons.Count);
    }

    public void Next() => SwitchTo((currentIndex + 1) % _weapons.Count);
    public void Prev() => SwitchTo((currentIndex - 1 + _weapons.Count) % _weapons.Count);

    public void SwitchTo(int index)
    {
        if (index < 0 || index >= _weapons.Count) return;

        // unsubscribe from old weapon events
        if (currentWeapon != null)
            currentWeapon.OnAmmoChanged -= HandleAmmoChanged;

        // deactivate old
        if (currentWeapon != null)
            currentWeapon.gameObject.SetActive(false);

        // activate new
        currentWeapon = _weapons[index];
        currentIndex = index;
        currentWeapon.gameObject.SetActive(true);

        // subscribe to ammo events
        currentWeapon.OnAmmoChanged += HandleAmmoChanged;

        // refresh HUD immediately
        HUDManager.Instance.ApplyWeapon(currentWeapon);
    }

    private void HandleAmmoChanged(Weapon w)
    {
        // live HUD update when you shoot/reload/pickup
        if (HUDManager.Instance != null)
            HUDManager.Instance.ApplyAmmo(w);
    }

    // OPTIONAL helpers you can call from pickups/inventory
    public Weapon GetCurrent() => currentWeapon;
    public List<Weapon> GetAll() => _weapons;
}
