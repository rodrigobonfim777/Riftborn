using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class EnemyDeath : MonoBehaviour
{
    [SerializeField] private GameObject experiencePickupPrefab;

    private EnemyHealth enemyHealth;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        enemyHealth.Died += HandleDeath;
    }

    private void OnDisable()
    {
        enemyHealth.Died -= HandleDeath;
    }

    private void HandleDeath()
    {
        // Phase enemies award XP directly through MatchStats, once per kill.
        if (GetComponent<EnemyKillReporter>() != null)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }
        if (experiencePickupPrefab != null)
        {
            Instantiate(experiencePickupPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("EnemyDeath requires an Experience Pickup Prefab to drop XP.", this);
        }

        gameObject.SetActive(false);
        Destroy(gameObject);
    }
}
