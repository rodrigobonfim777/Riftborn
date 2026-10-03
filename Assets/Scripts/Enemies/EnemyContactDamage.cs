using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyController), typeof(Collider2D))]
public class EnemyContactDamage : MonoBehaviour
{
    [SerializeField, Min(1)] private int damage = 10;
    [SerializeField, Min(0.01f)] private float damageInterval = 1f;
    [SerializeField, Min(0.1f)] private float attackDuration = 0.7f;
    [SerializeField, Min(0f)] private float bladeReach = 0.35f;

    private EnemyController controller;
    private Collider2D ownCollider;
    private PlayerHealth player;
    private Collider2D playerCollider;
    private float nextAttackTime;
    private float elapsed;
    private bool attacking;
    private bool hitApplied;

    public void SetDamage(int amount) => damage = Mathf.Max(1, amount);

    private void Awake()
    {
        controller = GetComponent<EnemyController>();
        ownCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (Time.deltaTime <= 0f) return;
        if (controller.Target == null) return;
        if (player == null || player.transform != controller.Target)
        {
            player = controller.Target.GetComponent<PlayerHealth>();
            playerCollider = controller.Target.GetComponent<Collider2D>();
        }
        if (player == null || player.CurrentHealth <= 0) return;
        if (attacking)
        {
            elapsed += Time.deltaTime;
            // Frame 4 of the seven-frame knife swing is the first impact pose.
            if (!hitApplied && elapsed >= attackDuration * (4f / 7f))
            {
                hitApplied = true;
                if (InRange()) player.TakeDamage(damage);
            }
            if (elapsed >= attackDuration) attacking = false;
            return;
        }
        if (Time.time < nextAttackTime || !InRange()) return;
        attacking = true;
        hitApplied = false;
        elapsed = 0f;
        nextAttackTime = Time.time + Mathf.Max(damageInterval, attackDuration);
        controller.PlayAttack(attackDuration);
    }

    private bool InRange()
    {
        if (playerCollider == null || !playerCollider.enabled || !ownCollider.enabled) return false;
        ColliderDistance2D distance = ownCollider.Distance(playerCollider);
        return distance.isValid && (distance.isOverlapped || distance.distance <= bladeReach);
    }

    private void OnDisable()
    {
        attacking = false;
        hitApplied = false;
        nextAttackTime = 0f;
    }
}
