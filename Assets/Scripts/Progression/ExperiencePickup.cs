using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CircleCollider2D))]
public class ExperiencePickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int experienceAmount = 5;
    [SerializeField, Min(0.1f)] private float collectionRadius = 1.5f;
    private bool collected;
    private PlayerExperience player;
    private PlayerHealth health;
    private MatchStats match;

    private void Start()
    {
        player = FindFirstObjectByType<PlayerExperience>();
        if (player != null) health = player.GetComponent<PlayerHealth>();
        match = FindFirstObjectByType<MatchStats>();
    }

    private void Update()
    {
        if (player != null && Vector2.Distance(transform.position, player.transform.position) <= collectionRadius)
            TryCollect(player);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out PlayerExperience experience)) TryCollect(experience);
    }

    private void TryCollect(PlayerExperience experience)
    {
        if (collected || Time.timeScale == 0 || (health != null && health.CurrentHealth <= 0)
            || (match != null && !match.CombatActive)) return;
        collected = true;
        experience.AddExperience(Mathf.Max(1, experienceAmount));
        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}

