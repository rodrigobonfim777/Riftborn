using UnityEngine;

[DisallowMultipleComponent]
public class GunterAreaAttack : MonoBehaviour
{
    private MatchStats match;
    private PlayerHealth player;
    private EnemyHealth health;
    private EnemyController movement;
    private LineRenderer warning;
    private float elapsed;
    public bool IsWarning { get; private set; }

    public void Initialize(MatchStats stats, SpriteRenderer ghost)
    {
        match = stats;
        player = stats.Player;
        health = GetComponent<EnemyHealth>();
        movement = GetComponent<EnemyController>();
        var visuals = gameObject.AddComponent<StageVisuals>();
        visuals.Initialize(ghost);
        Transform marker = new GameObject("Área de Gunter").transform;
        marker.SetParent(transform, false);
        marker.localScale = new Vector3(1f / transform.lossyScale.x, 1f / transform.lossyScale.y, 1);
        warning = visuals.Circle(marker, "Aviso de ataque", match.Rules.bossAreaRadius, new Color(1, 0.15f, 0.25f));
        warning.enabled = false;
        // Local sprite bounds preserve eye placement regardless of sprite PPU or boss scale.
        Bounds b = ghost.sprite.bounds;
        for (int i = -1; i <= 1; i += 2)
            visuals.Block(transform, "Olho vermelho", new Vector2(b.center.x + i * b.size.x * 0.13f,
                b.center.y + b.size.y * 0.1f), new Vector2(b.size.x * 0.09f, b.size.y * 0.08f),
                new Color(1, 0.05f, 0.1f), ghost.sortingOrder + 1);
    }

    private void Update()
    {
        if (match == null || !match.CombatActive || match.IsPaused || health.IsDead) return;
        elapsed += Time.deltaTime;
        float interval = Mathf.Max(1f, match.Rules.bossAttackInterval);
        float lead = Mathf.Clamp(match.Rules.bossWarningDuration, 0.1f, interval);
        if (!IsWarning && elapsed >= interval - lead)
        {
            IsWarning = true;
            warning.enabled = true;
            movement.enabled = false; // Telegraph stays still so the player can dodge it.
        }
        if (elapsed < interval) return;
        elapsed = 0;
        IsWarning = false;
        warning.enabled = false;
        if (Vector2.Distance(transform.position, player.transform.position) <= match.Rules.bossAreaRadius)
            player.TakeDamage(match.Rules.bossAreaDamage);
        if (match.CombatActive) movement.enabled = true;
    }

    private void OnDisable()
    {
        if (warning != null) warning.enabled = false;
        IsWarning = false;
    }
}

