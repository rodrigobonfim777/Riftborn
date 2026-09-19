using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class MatchStats : MonoBehaviour
{
    public const int GhostTarget = 50;
    public int KillCount { get; private set; }
    public int GhostKills { get; private set; }
    public bool BossSpawned { get; private set; }
    public bool Victory { get; private set; }
    public bool GameOver { get; private set; }
    public bool DeathMessageVisible { get; private set; }
    public bool Finished => Victory || GameOver;
    public EnemyHealth Boss { get; private set; }
    public event Action KillsChanged;
    private int ghostsSpawned;
    private PlayerHealth playerHealth;
    private PlayerExperience experience;

    private void Start()
    {
        playerHealth = FindFirstObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            experience = playerHealth.GetComponent<PlayerExperience>();
            playerHealth.Died += OnPlayerDied;
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null) playerHealth.Died -= OnPlayerDied;
    }

    public bool TryReserveGhost()
    {
        if (Finished || ghostsSpawned >= GhostTarget) return false;
        ghostsSpawned++;
        return true;
    }

    public bool TryReserveBoss()
    {
        if (Finished || GhostKills < GhostTarget || BossSpawned) return false;
        BossSpawned = true;
        KillsChanged?.Invoke();
        return true;
    }

    public void SetBoss(EnemyHealth boss) => Boss = boss;

    public void RegisterKill(bool isBoss = false)
    {
        if (Finished) return;
        KillCount++;
        if (experience != null) experience.AddExperience(isBoss ? 50 : 5);
        if (isBoss)
        {
            Victory = true;
            StopCombat();
        }
        else GhostKills++;
        KillsChanged?.Invoke();
    }

    private void OnPlayerDied()
    {
        if (Finished) return;
        GameOver = true;
        StopCombat();
        StartCoroutine(ShowDeathMessage());
    }

    private System.Collections.IEnumerator ShowDeathMessage()
    {
        yield return new WaitForSecondsRealtime(3f);
        DeathMessageVisible = true;
        KillsChanged?.Invoke();
    }

    private void StopCombat()
    {
        foreach (EnemyController enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
            enemy.enabled = false;
        foreach (EnemyContactDamage contact in FindObjectsByType<EnemyContactDamage>(FindObjectsSortMode.None))
            contact.enabled = false;
        foreach (Weapon weapon in FindObjectsByType<Weapon>(FindObjectsSortMode.None)) weapon.enabled = false;
        foreach (Projectile projectile in FindObjectsByType<Projectile>(FindObjectsSortMode.None))
        {
            projectile.gameObject.SetActive(false);
            Destroy(projectile.gameObject);
        }
    }

    private void OnGUI()
    {
        if (Boss != null && !Finished)
            GUI.Box(new Rect(Screen.width / 2f - 150, 20, 300, 35), $"REI FANTASMA — {Boss.CurrentHealth}/{Boss.MaximumHealth} PV");
        if (!Victory && !DeathMessageVisible) return;
        Rect panel = new Rect(Screen.width / 2f - 160, Screen.height / 2f - 70, 320, 140);
        GUI.Box(panel, Victory ? "FASE 1 CONCLUÍDA!" : "VOCÊ MORREU");
        if (GUI.Button(new Rect(panel.x + 60, panel.y + 65, 200, 45), "Jogar novamente"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
