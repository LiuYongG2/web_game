using UnityEngine;

[System.Serializable]
public class WeaponData
{
    public string weaponName;
    public int magazineSize;
    public int maxReserve;
    public float fireRate;
    public float spread;
    public int raysPerShot;
    public float recoilUp;
    public float recoilSide;
    public int damage;
    public float range;
    public float reloadTime;
    public bool isAutomatic;
    public AudioClip fireSound;
    public AudioClip reloadSound;
    public GameObject muzzleFlashPrefab;
    public GameObject impactEffectPrefab;
}

public class WeaponBase : MonoBehaviour
{
    public WeaponData data;
    public Transform muzzlePoint;
    public Transform aimDownSightPosition;
    public Animator animator;

    private int currentMag;
    private int currentReserve;
    private float nextFireTime;
    private bool isReloading;
    private PlayerCamera playerCamera;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0;
    }

    public void Initialize(PlayerCamera cam)
    {
        playerCamera = cam;
        currentMag = data.magazineSize;
        currentReserve = data.maxReserve;
    }

    public bool TryFire()
    {
        if (isReloading) return false;
        if (Time.time < nextFireTime) return false;
        if (currentMag <= 0) { StartReload(); return false; }

        currentMag--;
        nextFireTime = Time.time + data.fireRate;

        for (int i = 0; i < data.raysPerShot; i++)
        {
            Vector3 spreadDir = GetSpreadDirection();
            if (Physics.Raycast(muzzlePoint.position, spreadDir, out RaycastHit hit, data.range))
            {
                PlayerHealth health = hit.collider.GetComponentInParent<PlayerHealth>();
                if (health != null)
                {
                    bool headshot = hit.collider.CompareTag("Head");
                    health.TakeDamage(headshot ? data.damage * 3 : data.damage);
                }

                AIController ai = hit.collider.GetComponentInParent<AIController>();
                if (ai != null)
                {
                    bool headshot = hit.point.y > hit.collider.transform.position.y + 1.3f;
                    ai.TakeDamage(headshot ? data.damage * 3 : data.damage);
                }

                if (data.impactEffectPrefab != null)
                    Instantiate(data.impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }

        if (data.muzzleFlashPrefab != null && muzzlePoint != null)
        {
            GameObject flash = Instantiate(data.muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
            Destroy(flash, 0.05f);
        }

        if (data.fireSound != null) audioSource.PlayOneShot(data.fireSound);
        if (playerCamera != null) playerCamera.AddRecoil(data.recoilUp, data.recoilSide);
        if (animator != null) animator.SetTrigger("Fire");

        return true;
    }

    Vector3 GetSpreadDirection()
    {
        Vector3 dir = muzzlePoint.forward;
        dir += muzzlePoint.right * Random.Range(-data.spread, data.spread);
        dir += muzzlePoint.up * Random.Range(-data.spread, data.spread);
        return dir.normalized;
    }

    public void StartReload()
    {
        if (isReloading || currentMag == data.magazineSize) return;
        if (currentReserve <= 0 && data.maxReserve > 0) return;
        isReloading = true;
        if (data.reloadSound != null) audioSource.PlayOneShot(data.reloadSound);
        if (animator != null) animator.SetTrigger("Reload");
        Invoke(nameof(FinishReload), data.reloadTime);
    }

    void FinishReload()
    {
        int needed = data.magazineSize - currentMag;
        if (data.maxReserve <= 0)
        {
            currentMag = data.magazineSize;
        }
        else
        {
            int add = Mathf.Min(needed, currentReserve);
            currentMag += add;
            currentReserve -= add;
        }
        isReloading = false;
    }

    public void AddReserve(int amount)
    {
        currentReserve = Mathf.Min(currentReserve + amount, data.maxReserve > 0 ? data.maxReserve * 2 : int.MaxValue);
    }

    public int CurrentMag => currentMag;
    public int CurrentReserve => currentReserve;
    public bool IsReloading => isReloading;
    public bool IsAutomatic => data.isAutomatic;
}
