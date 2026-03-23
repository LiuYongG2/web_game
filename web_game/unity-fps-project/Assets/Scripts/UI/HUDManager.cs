using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    [Header("Health")]
    public Image healthBar;
    public Image armorBar;
    public TextMeshProUGUI healthText;
    public Image damageOverlay;

    [Header("Weapon")]
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI reserveText;
    public GameObject reloadIndicator;

    [Header("Score")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI killsText;
    public TextMeshProUGUI comboText;
    public TextMeshProUGUI timerText;

    [Header("Crosshair")]
    public RectTransform[] crosshairLines;
    public Image crosshairDot;
    public float crosshairSpreadAmount = 10f;

    [Header("Hit Feedback")]
    public GameObject hitMarker;
    public TextMeshProUGUI hitPointsText;

    [Header("Killfeed")]
    public Transform killfeedParent;
    public GameObject killfeedEntryPrefab;

    [Header("References")]
    public WeaponManager weaponManager;
    public PlayerHealth playerHealth;

    private float damageOverlayAlpha;
    private float crosshairSpread;
    private float hitMarkerTimer;

    void Update()
    {
        UpdateWeaponHUD();
        UpdateDamageOverlay();
        UpdateCrosshair();
        UpdateHitMarker();
    }

    void UpdateWeaponHUD()
    {
        if (weaponManager == null || weaponManager.CurrentWeapon == null) return;
        var w = weaponManager.CurrentWeapon;
        if (weaponNameText) weaponNameText.text = w.data.weaponName;
        if (ammoText) ammoText.text = w.CurrentMag.ToString();
        if (reserveText) reserveText.text = w.data.maxReserve <= 0 ? "∞" : w.CurrentReserve.ToString();
        if (reloadIndicator) reloadIndicator.SetActive(w.IsReloading);
    }

    void UpdateDamageOverlay()
    {
        damageOverlayAlpha = Mathf.Lerp(damageOverlayAlpha, 0, Time.deltaTime * 5f);
        if (damageOverlay)
        {
            var c = damageOverlay.color;
            c.a = damageOverlayAlpha;
            damageOverlay.color = c;
        }
    }

    void UpdateCrosshair()
    {
        crosshairSpread = Mathf.Lerp(crosshairSpread, 0, Time.deltaTime * 12f);
        if (crosshairLines == null) return;
        float offset = 6f + crosshairSpread;
        if (crosshairLines.Length >= 4)
        {
            crosshairLines[0].anchoredPosition = new Vector2(-offset - 16, 0);
            crosshairLines[1].anchoredPosition = new Vector2(offset, 0);
            crosshairLines[2].anchoredPosition = new Vector2(0, offset);
            crosshairLines[3].anchoredPosition = new Vector2(0, -offset - 16);
        }
    }

    void UpdateHitMarker()
    {
        if (hitMarkerTimer > 0)
        {
            hitMarkerTimer -= Time.deltaTime;
            if (hitMarkerTimer <= 0 && hitMarker) hitMarker.SetActive(false);
        }
    }

    public void OnPlayerDamaged()
    {
        damageOverlayAlpha = 0.4f;
    }

    public void OnHealthChanged(int current, int max)
    {
        if (healthBar) healthBar.fillAmount = (float)current / max;
        if (healthText) healthText.text = $"{current}";
        float pct = (float)current / max;
        if (healthBar)
        {
            healthBar.color = pct > 0.6f ? new Color(0.29f, 0.85f, 0.5f)
                            : pct > 0.3f ? new Color(0.98f, 0.75f, 0.15f)
                            : new Color(0.93f, 0.27f, 0.27f);
        }
    }

    public void OnArmorChanged(int current)
    {
        if (armorBar) armorBar.fillAmount = (float)current / 50f;
    }

    public void ShowHitMarker(bool headshot, int points)
    {
        if (hitMarker) hitMarker.SetActive(true);
        hitMarkerTimer = 0.2f;
        crosshairSpread = crosshairSpreadAmount;
        if (hitPointsText)
        {
            hitPointsText.text = (points > 0 ? "+" : "") + points + (headshot ? " HEADSHOT" : "");
            hitPointsText.color = headshot ? Color.red : new Color(1f, 0.84f, 0f);
        }
    }

    public void AddKillfeed(string killerName, string victimName, bool headshot)
    {
        if (killfeedParent == null || killfeedEntryPrefab == null) return;
        var entry = Instantiate(killfeedEntryPrefab, killfeedParent);
        var text = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (text) text.text = $"{killerName} {(headshot ? "🎯" : "→")} {victimName}";
        Destroy(entry, 5f);
    }

    public void UpdateScore(int score, int kills, int headshots)
    {
        if (scoreText) scoreText.text = score.ToString();
        if (killsText) killsText.text = $"击杀 {kills} | 爆头 {headshots}";
    }

    public void UpdateTimer(float seconds)
    {
        if (timerText)
        {
            timerText.text = Mathf.CeilToInt(seconds).ToString();
            timerText.color = seconds <= 10 ? Color.red : Color.white;
        }
    }

    public void ShowCombo(int combo)
    {
        if (comboText)
        {
            comboText.text = combo > 0 ? $"×{combo + 1} COMBO" : "";
        }
    }
}
