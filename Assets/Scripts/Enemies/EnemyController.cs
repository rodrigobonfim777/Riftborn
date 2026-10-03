using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField, Min(0f)] private float movementSpeed = 3f;
    [SerializeField, Min(1f)] private float animationFramesPerSecond = 10f;
    [SerializeField] private Sprite[] idleFrames;
    [SerializeField] private Sprite[] walkFrames;
    [SerializeField] private Sprite[] attackFrames;

    private Rigidbody2D body;
    private SpriteRenderer spriteRenderer;
    private float animationElapsed;
    private float attackElapsed;
    private float attackDuration;
    private bool moving;
    public Transform Target => target;
    public bool IsAttacking { get; private set; }

    private void Awake()
    {
        movementSpeed = Mathf.Max(0f, movementSpeed);
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetMovementSpeed(float speed) => movementSpeed = Mathf.Max(0f, speed);
    public void SetTarget(Transform newTarget) => target = newTarget;

    public void PlayAttack(float duration)
    {
        IsAttacking = true;
        attackElapsed = 0f;
        attackDuration = Mathf.Max(0.01f, duration);
        body.linearVelocity = Vector2.zero;
        FaceTarget();
        ShowFrame(attackFrames, 0);
    }

    private void FixedUpdate()
    {
        moving = false;
        if (target == null || IsAttacking)
        {
            body.linearVelocity = Vector2.zero;
            return;
        }
        Vector2 offset = (Vector2)target.position - body.position;
        float speed = Mathf.Min(movementSpeed, offset.magnitude / Time.fixedDeltaTime);
        moving = speed > 0.01f;
        body.linearVelocity = offset.normalized * speed;
    }

    private void LateUpdate()
    {
        if (IsAttacking)
        {
            attackElapsed += Time.deltaTime;
            if (attackElapsed < attackDuration)
            {
                int count = attackFrames == null ? 0 : attackFrames.Length;
                ShowFrame(attackFrames, Mathf.Min(count - 1,
                    Mathf.FloorToInt(attackElapsed / attackDuration * count)));
                return;
            }
            IsAttacking = false;
        }
        FaceTarget();
        Sprite[] frames = moving ? walkFrames : idleFrames;
        if (frames == null || frames.Length == 0) return;
        float rate = Mathf.Max(1f, animationFramesPerSecond);
        // Keep the cycle advancing even when contact briefly changes the movement state.
        animationElapsed = (animationElapsed + Time.deltaTime) % (frames.Length / rate);
        ShowFrame(frames, Mathf.FloorToInt(animationElapsed * rate));
    }

    private void FaceTarget()
    {
        if (spriteRenderer == null || target == null) return;
        float horizontal = target.position.x - transform.position.x;
        if (Mathf.Abs(horizontal) > 0.05f) spriteRenderer.flipX = horizontal < 0f;
    }

    private void ShowFrame(Sprite[] frames, int index)
    {
        if (spriteRenderer == null || frames == null || frames.Length == 0) return;
        Sprite frame = frames[Mathf.Clamp(index, 0, frames.Length - 1)];
        if (frame != null) spriteRenderer.sprite = frame;
    }

    private void OnDisable()
    {
        IsAttacking = false;
        animationElapsed = 0f;
        moving = false;
        if (body != null) body.linearVelocity = Vector2.zero;
    }
}
