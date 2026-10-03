using System;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maximumHealth = 10;
    public int MaximumHealth => maximumHealth;
    public int CurrentHealth { get; private set; }
    public bool IsDead => CurrentHealth <= 0;
    public event Action Died;

    private void Awake() => ConfigureHealth(maximumHealth);

    public void ConfigureHealth(int health)
    {
        maximumHealth = Mathf.Max(1, health);
        CurrentHealth = maximumHealth;
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0 || IsDead) return;
        CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        if (IsDead) Died?.Invoke();
    }
}

