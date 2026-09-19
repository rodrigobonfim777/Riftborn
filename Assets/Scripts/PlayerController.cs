using UnityEngine;
using UnityEngine.InputSystem;

// Top-down movement: configure Rigidbody2D with zero gravity and frozen Z rotation.
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("Movement speed in Unity units per second.")]
    [SerializeField, Min(0f)] private float movementSpeed = 5f;

    private Rigidbody2D body;
    private Vector2 movementDirection;
    private Collider2D bodyCollider;

    public Vector2 MovementDirection => movementDirection;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        movementDirection = Vector2.zero;

        if (keyboard == null)
        {
            return;
        }

        float horizontal = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1f : 0f)
            - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1f : 0f);
        float vertical = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1f : 0f)
            - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1f : 0f);

        // Keep diagonal movement at the same speed as horizontal/vertical movement.
        movementDirection = new Vector2(horizontal, vertical).normalized;
    }

    private void FixedUpdate()
    {
        // Velocity is measured in units per second; do not multiply by delta time.
        Vector2 velocity = movementDirection * movementSpeed;
        HouseFloor map = HouseFloor.Active;
        if (map != null)
        {
            Vector2 margin = bodyCollider != null ? (Vector2)bodyCollider.bounds.extents : Vector2.one * 0.5f;
            Vector2 offset = bodyCollider != null ? (Vector2)bodyCollider.bounds.center - body.position : Vector2.zero;
            margin += Vector2.one * 0.5f;
            Vector2 center = body.position + offset;
            Vector2 clamped = map.Clamp(center, margin);
            if ((clamped - center).sqrMagnitude > 0.000001f)
                body.position = clamped - offset;
            Vector2 next = map.Clamp(clamped + velocity * Time.fixedDeltaTime, margin);
            velocity = (next - clamped) / Time.fixedDeltaTime;
        }
        body.linearVelocity = velocity;
    }

    private void OnDisable()
    {
        movementDirection = Vector2.zero;

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }
    }
}
