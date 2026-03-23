using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 100;
    public int maxArmor = 50;

    [Header("Events")]
    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent<int> OnArmorChanged;
    public UnityEvent OnDeath;
    public UnityEvent OnDamaged;

    private int currentHealth;
    private int currentArmor;
    private bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
        currentArmor = maxArmor;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnArmorChanged?.Invoke(currentArmor);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        int remaining = amount;

        if (currentArmor > 0)
        {
            int absorbed = Mathf.Min(currentArmor, Mathf.CeilToInt(remaining * 0.6f));
            currentArmor -= absorbed;
            remaining -= absorbed;
            OnArmorChanged?.Invoke(currentArmor);
        }

        currentHealth = Mathf.Max(0, currentHealth - remaining);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnDamaged?.Invoke();

        if (currentHealth <= 0 && !isDead)
        {
            isDead = true;
            OnDeath?.Invoke();
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void AddArmor(int amount)
    {
        currentArmor = Mathf.Min(maxArmor, currentArmor + amount);
        OnArmorChanged?.Invoke(currentArmor);
    }

    public void Reset()
    {
        isDead = false;
        currentHealth = maxHealth;
        currentArmor = maxArmor;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnArmorChanged?.Invoke(currentArmor);
    }

    public int Health => currentHealth;
    public int Armor => currentArmor;
    public bool IsDead => isDead;
}
