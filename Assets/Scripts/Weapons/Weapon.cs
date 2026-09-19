using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
[DefaultExecutionOrder(100)]
public class Weapon : MonoBehaviour
{
    public event System.Action<Vector2> ShotFired;
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField, Min(1)] private int damage = 10;
    [Tooltip("Projectile speed in Unity units per second.")]
    [SerializeField, Min(0.01f)] private float projectileSpeed = 10f;
    [Tooltip("Maximum number of automatic shots per second.")]
    [SerializeField, Min(0.01f)] private float shotsPerSecond = 4f;

    private PlayerExperience experience;
    private PlayerHealth ownerHealth;
    private Camera aimCamera;
    private float nextShotTime;

    public int CurrentDamage => (int)System.Math.Min((long)damage + (experience != null ? experience.DamageBonus : 0), int.MaxValue);
    public Vector3 AimOrigin => firePoint != null ? firePoint.position : transform.position;
    public Vector3 AimPosition { get; private set; }
    public Vector2 AimDirection { get; private set; }
    public bool HasAim { get; private set; }

    private void Awake()
    {
        if (GetComponent<WeaponAimIndicator>() == null)
            gameObject.AddComponent<WeaponAimIndicator>();
        experience = GetComponentInParent<PlayerExperience>();
        ownerHealth = GetComponentInParent<PlayerHealth>();
        damage = Mathf.Max(1, damage);
        projectileSpeed = Mathf.Max(0.01f, projectileSpeed);
        shotsPerSecond = Mathf.Max(0.01f, shotsPerSecond);

        if (projectilePrefab == null || firePoint == null)
        {
            Debug.LogError("Weapon requires a Projectile Prefab and Fire Point.", this);
            enabled = false;
        }
    }

    // Resolve the cursor after CameraFollow has moved the camera for this frame.
    private void LateUpdate()
    {
        HasAim = false;
        if (Time.timeScale == 0f || (ownerHealth != null && ownerHealth.CurrentHealth <= 0))
            return;

        Mouse mouse = Mouse.current;
        if (aimCamera == null || !aimCamera.isActiveAndEnabled)
            aimCamera = Camera.main;
        if (mouse == null || aimCamera == null)
            return;

        Ray ray = aimCamera.ScreenPointToRay(mouse.position.ReadValue());
        Plane aimPlane = new Plane(Vector3.forward, transform.position);
        if (!aimPlane.Raycast(ray, out float distance))
            return;

        AimPosition = ray.GetPoint(distance);
        Vector2 aimDirection = AimPosition - transform.position;
        if (aimDirection.sqrMagnitude > 0.000001f)
        {
            float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        // Use the muzzle position after rotation for both the guide and the shot.
        Vector2 shotDirection = AimPosition - AimOrigin;
        if (shotDirection.sqrMagnitude <= 0.000001f)
            return;

        AimDirection = shotDirection.normalized;
        HasAim = true;
        if (Time.time >= nextShotTime)
        {
            Shoot();
            nextShotTime = Time.time + 1f / shotsPerSecond;
        }
    }

    private void Shoot()
    {
        float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;
        Projectile projectile = Instantiate(projectilePrefab, AimOrigin, Quaternion.Euler(0f, 0f, angle));
        projectile.Initialize(AimDirection, projectileSpeed, CurrentDamage);
        ShotFired?.Invoke(AimDirection);
    }

    private void OnDisable() => HasAim = false;
}
