using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerController), typeof(SpriteRenderer))]
public class PlayerSpriteAnimation : MonoBehaviour
{
    [SerializeField, Min(1f)] private float framesPerSecond = 8f;
    [SerializeField, Min(0.01f)] private float shotDuration = 0.18f;
    [SerializeField] private Sprite[] walkDown;
    [SerializeField] private Sprite[] walkUp;
    [SerializeField] private Sprite[] walkLeft;
    [SerializeField] private Sprite[] walkRight;
    [SerializeField] private Sprite[] shootDown;
    [SerializeField] private Sprite[] shootUp;
    [SerializeField] private Sprite[] shootLeft;
    [SerializeField] private Sprite[] shootRight;

    [SerializeField] private Sprite[] deathFrames;
    [SerializeField] private Sprite deadSprite;
    private bool dying;
    private float deathElapsed;

    public void PlayDeath()
    {
        dying = true;
        deathElapsed = 0f;
        shooting = false;
        spriteRenderer.flipX = false;
    }

    private enum Facing { Down, Up, Left, Right }
    private PlayerController controller;
    private SpriteRenderer spriteRenderer;
    private Weapon weapon;
    private Facing facing;
    private Facing shotFacing;
    private float walkElapsed;
    private float shotElapsed;
    private bool shooting;
    private bool wasMoving;
    private readonly System.Collections.Generic.List<Sprite> normalizedSideFrames = new System.Collections.Generic.List<Sprite>();

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        weapon = GetComponentInChildren<Weapon>();
        // Side poses come from a taller row; retain the same on-screen body height.
        walkLeft = NormalizeSideFrames(walkLeft);
        walkRight = NormalizeSideFrames(walkRight);
    }

    private void OnEnable()
    {
        if (weapon != null)
            weapon.ShotFired += OnShotFired;
    }

    private void OnDisable()
    {
        if (weapon != null)
            weapon.ShotFired -= OnShotFired;
        shooting = false;
        wasMoving = false;
        walkElapsed = 0f;
    }

    private void OnShotFired(Vector2 direction)
    {
        shotFacing = DirectionToFacing(direction);
        shotElapsed = 0f;
        shooting = true;
    }

    private void LateUpdate()
    {
        if (dying)
        {
            deathElapsed += Time.unscaledDeltaTime;
            if (deadSprite != null && (deathElapsed >= 0.8f || deathFrames == null || deathFrames.Length == 0))
            {
                spriteRenderer.flipX = false;
                spriteRenderer.sprite = deadSprite;
            }
            else if (deathFrames != null && deathFrames.Length > 0)
                ShowFrame(deathFrames, Mathf.Min(deathFrames.Length - 1,
                    Mathf.FloorToInt(deathElapsed / 0.8f * deathFrames.Length)));
            return;
        }
        Vector2 direction = controller.MovementDirection;
        bool moving = controller.isActiveAndEnabled && direction.sqrMagnitude > 0.001f;
        Facing nextFacing = moving ? DirectionToFacing(direction) : facing;
        if (!moving || !wasMoving || nextFacing != facing)
            walkElapsed = 0f;
        else
            walkElapsed += Time.deltaTime;

        facing = nextFacing;
        wasMoving = moving;

        if (shooting)
        {
            Sprite[] shots = Frames(shotFacing, true);
            float duration = Mathf.Max(0.01f, shotDuration);
            if (shotElapsed < duration && shots != null && shots.Length > 0)
            {
                int shotFrame = Mathf.Min(shots.Length - 1,
                    Mathf.FloorToInt(shotElapsed / duration * shots.Length));
                ShowFrame(shots, shotFrame);
                shotElapsed += Time.deltaTime;
                // Remember the aim direction when firing while standing still.
                if (!moving) facing = shotFacing;
                return;
            }
            shooting = false;
        }

        Sprite[] walk = Frames(facing, false);
        ShowFrame(walk, moving ? Mathf.FloorToInt(walkElapsed * Mathf.Max(1f, framesPerSecond)) : 0);
        // Both side cycles use the left-facing stride row.
        spriteRenderer.flipX = facing == Facing.Right;
    }


    private Sprite[] NormalizeSideFrames(Sprite[] source)
    {
        if (source == null) return null;
        Sprite[] result = (Sprite[])source.Clone();
        for (int i = 0; i < result.Length; i++)
        {
            Sprite frame = result[i];
            if (frame == null || Mathf.Abs(frame.rect.height - 174f) > 0.1f) continue;
            Sprite normalized = Sprite.Create(frame.texture, frame.rect,
                new Vector2(frame.pivot.x / frame.rect.width, frame.pivot.y / frame.rect.height),
                frame.pixelsPerUnit * 174f / 148f);
            normalized.name = frame.name;
            result[i] = normalized;
            normalizedSideFrames.Add(normalized);
        }
        return result;
    }

    private void OnDestroy()
    {
        foreach (Sprite frame in normalizedSideFrames) Destroy(frame);
    }

    private static Facing DirectionToFacing(Vector2 direction)
    {
        if (Mathf.Abs(direction.x) >= Mathf.Abs(direction.y) && direction.x != 0f)
            return direction.x > 0f ? Facing.Right : Facing.Left;
        return direction.y > 0f ? Facing.Up : Facing.Down;
    }

    private Sprite[] Frames(Facing direction, bool attack)
    {
        switch (direction)
        {
            case Facing.Up: return attack ? shootUp : walkUp;
            case Facing.Left: return attack ? shootLeft : walkLeft;
            case Facing.Right: return attack ? shootRight : walkRight;
            default: return attack ? shootDown : walkDown;
        }
    }

    private void ShowFrame(Sprite[] frames, int frame)
    {
        if (frames == null || frames.Length == 0) return;
        Sprite sprite = frames[frame % frames.Length];
        if (sprite == null) return;
        spriteRenderer.flipX = false;
        spriteRenderer.sprite = sprite;
    }
}
