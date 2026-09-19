using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;

    public int MaxHealth => maxHealth;
    public int CurrentHealth { get; private set; }
    public event Action HealthChanged;
    public event Action Died;

    private void Awake()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        CurrentHealth = maxHealth;
        HealthChanged?.Invoke();
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0)
            return;

        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        bool died = CurrentHealth == 0;
        HealthChanged?.Invoke();

        if (died)
        {
            GetComponent<PlayerController>().enabled = false;
            foreach (Weapon weapon in GetComponentsInChildren<Weapon>()) weapon.enabled = false;
            foreach (Collider2D collider in GetComponents<Collider2D>()) collider.enabled = false;
            PlayerSpriteAnimation animation = GetComponent<PlayerSpriteAnimation>();
            if (animation != null) animation.PlayDeath();
            Died?.Invoke();
        }
    }
}
