using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    public WeaponBase[] weapons;
    public PlayerCamera playerCamera;
    public int currentWeaponIndex;

    void Start()
    {
        foreach (var w in weapons)
        {
            w.Initialize(playerCamera);
            w.gameObject.SetActive(false);
        }
        if (weapons.Length > 0) EquipWeapon(0);
    }

    void Update()
    {
        if (weapons.Length == 0) return;
        var current = weapons[currentWeaponIndex];

        if (current.IsAutomatic)
        {
            if (Input.GetMouseButton(0)) current.TryFire();
        }
        else
        {
            if (Input.GetMouseButtonDown(0)) current.TryFire();
        }

        if (Input.GetKeyDown(KeyCode.R)) current.StartReload();

        for (int i = 0; i < Mathf.Min(weapons.Length, 6); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) EquipWeapon(i);
        }

        if (Input.GetAxis("Mouse ScrollY") > 0)
            EquipWeapon((currentWeaponIndex - 1 + weapons.Length) % weapons.Length);
        else if (Input.GetAxis("Mouse ScrollY") < 0)
            EquipWeapon((currentWeaponIndex + 1) % weapons.Length);
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weapons.Length || index == currentWeaponIndex) return;
        weapons[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex = index;
        weapons[currentWeaponIndex].gameObject.SetActive(true);
    }

    public WeaponBase CurrentWeapon => weapons.Length > 0 ? weapons[currentWeaponIndex] : null;
}
