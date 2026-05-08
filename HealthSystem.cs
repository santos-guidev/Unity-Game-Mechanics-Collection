using UnityEngine;
using System;

public interface IDamageable
{
    void TakeDamage(float amount);
    void Heal(float amount);
    bool IsAlive { get; }
    event Action OnDeath;
    event Action<float> OnHealthChanged;
}

public class HealthSystem : MonoBehaviour, IDamageable
{
    [SerializeField] private float _maxHealth = 100f;
    private float _currentHealth;

    public bool IsAlive => _currentHealth > 0;

    public event Action OnDeath;
    public event Action<float> OnHealthChanged;

    void Awake()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        _currentHealth -= amount;
        _currentHealth = Mathf.Max(_currentHealth, 0);

        OnHealthChanged?.Invoke(_currentHealth);

        if (_currentHealth <= 0)
        {
            OnDeath?.Invoke();
            Debug.Log($"{gameObject.name} has died.");
        }
    }

    public void Heal(float amount)
    {
        if (!IsAlive) return;

        _currentHealth += amount;
        _currentHealth = Mathf.Min(_currentHealth, _maxHealth);

        OnHealthChanged?.Invoke(_currentHealth);
    }

    public float GetCurrentHealth() => _currentHealth;
    public float GetMaxHealth() => _maxHealth;
    public float GetHealthPercentage() => _currentHealth / _maxHealth;
}
